# ustats Script (c) noobtuts.com 2015
#
import UnityEditor
import UnityEngine
import System # Delegate.Combine
import System.IO
import System.Collections.Generic

# shortcuts ####################################################################
def eq_lower(a as string, b as string):
    return a.ToLower() == b.ToLower()

# functional programming #######################################################
def add(a as int, b as int):
    return a + b

# map(iseven, [1, 2, 3, 4]) => [False, True, False, True]
def map(fn as ICallable, coll):
    return [fn(x) for x in coll]

# reduce (copied from python)
#reduce(add, [0, 1, 2, 3], 0) # => 6
#reduce({x as int,y as int|x+y}, [0, 1, 2, 3], 0) => 6
#reduce(add, [], 0) # => 0
#reduce(add, (0, 1), 0) # 1
def reduce(fn as ICallable, coll, initializer):
    for x in coll:
        initializer = fn(initializer, x)
    return initializer

# filter
#filter(is_even, [0, 1, 2]) # => [0, 2]
#filter({x as int|x%2==0}, [0, 1, 2]) # => [0, 2]
def filter(fn as ICallable, coll):
    return [x for x in coll if fn(x)]

# other helper functions #######################################################
# sum
#sum([0, 1, 2, 3]) # => 6
def sum(coll) as int:
    return reduce({x as int, y as int | x+y}, coll, 0)

# increase dictionary value safely
# TODO return value and immutable
def dict_safeadd(dict as Dictionary[of string, int], key as string, n as int):
    if key in dict:
        dict[key] += n
    else:
        dict[key] = n

# returns dict[value] or 0
def dict_get(dict as Dictionary[of string, int], key as string):
    #return dict[key] or 0 # doesn't work in Unity
    if key in dict:
        return dict[key]
    return 0

# add dict values to dict
# TODO return value and immutable
def dicts_add(d1 as Dictionary[of string, int], d2 as Dictionary[of string, int]):
    map({k | dict_safeadd(d1, k, d2[k])}, d2.Keys)

# returns dict of [extension, count]
def count_filetypes(dir as string) as Dictionary[of string, int]:
    res = Dictionary[of string, int]()
    
    # create dict from filesextensions in this directory
    # TODO reduce with dicts_add?
    map({f | dict_safeadd(res, Path.GetExtension(f), 1)}, Directory.GetFiles(dir))
    
    # continue recursively (add each dict to res)
    dicts = map(count_filetypes, Directory.GetDirectories(dir))
    map({d as Dictionary[of string, int] | dicts_add(res, d)}, dicts)
    return res

def file_loc(fname as string):
    return File.ReadAllLines(fname).Length

# returns dict of [extension, loc]
def count_loc(dir as string, formats as (string)) as Dictionary[of string, int]:
    res = Dictionary[of string, int]()
    
    # create dict from filesextensions in this directory
    # TODO reduce with dicts_add?
    #map({f | dict_safeadd(res, Path.GetExtension(f), file_loc(f))}, Directory.GetFiles(dir))
    for f in Directory.GetFiles(dir):
        ext = Path.GetExtension(f)
        if ext in formats:
            dict_safeadd(res, ext, file_loc(f))
    
    # continue recursively (add each dict to res)
    dicts = map({d|count_loc(d, formats)}, Directory.GetDirectories(dir))
    map({d as Dictionary[of string, int] | dicts_add(res, d)}, dicts)
    return res

# 1 second, 2 seconds etc.
def time_suffix(t as int):
    if t == 1:
        return ""
    return "s"

# display the two biggest units
# note: displaying seconds feels to nervous
def format_seconds(seconds as single):
    t = TimeSpan.FromSeconds(seconds)
    #return t.Days + "d, " + t.Hours + "h, " + t.Minutes + "m, " + t.Seconds + "s"
    if t.Days:
        return t.Days + " day" + time_suffix(t.Days) + " and " + t.Hours + " hour" + time_suffix(t.Hours)
    elif t.Hours:
        return t.Hours + " hour" + time_suffix(t.Hours) + " and " + t.Minutes + " minute" + time_suffix(t.Minutes)
    return t.Minutes + " minute" + time_suffix(t.Minutes)

def save(fname as string, time_editing as double, time_playing as double):
    using writer = BinaryWriter(File.Open(fname, FileMode.Create)):
        writer.Write(time_editing)
        writer.Write(time_playing)

def load(fname as string):
    if File.Exists(fname):
        using reader = BinaryReader(File.Open(fname, FileMode.Open)):
            time_editing = reader.ReadDouble()
            time_playing = reader.ReadDouble()
            return time_editing, time_playing
    return 0.0, 0.0

[ExecuteInEditMode]
class ustats(EditorWindow):
    # note: all values have to be static, otherwise they are invalid as soon as
    #       the editor window is closed and reopened
    
    # stats file
    static fname = "Library/ustats.data"    
    
    # formats: https://unity3d.com/unity/editor
    static fmt_scripts = (".cs", ".js", ".boo")
    static fmt_textures = (".bmp", ".dds", ".gif", ".iff", ".jpg", ".jpeg", ".pict", ".png", ".psd", ".tga", ".tiff")
    static fmt_sounds = (".aiff", ".ogg", ".mp3", ".wav", ".mod", ".it", ".sm3")
    static fmt_videos = (".mov", ".avi", ".asf", ".mpg", ".mpeg", ".mp4")
    static fmt_texts = (".txt", ".htm", ".html", ".xml", ".bytes")
    
    # assets amounts
    static count_scripts = 0
    static count_textures = 0
    static count_sounds = 0
    static count_videos = 0
    static count_texts = 0
    
    # loc
    static loc_boo = 0
    static loc_cs = 0
    static loc_js = 0
    
    # time counters
    static time_editing as double = 0.0
    static time_playing as double = 0.0
    
    # on project start (static because Tick is ended after pressing play, hence
    # needs to be recreated)
    static initialized = false
    
    # time since last save, save interval
    static lastsave = 0.0
    static saveinterval = 10.0 # 1 minute is too much for play/edit/play/... 
        
    # Editor Window ############################################################    
    # called when opening the window from the menu.
    # not called when starting unity.
    [MenuItem("Window/ustats")]
    public static def Init():
        # Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(ustats)).Show()
    
    def OnGUI():
        GUILayout.BeginVertical()
        
        GUILayout.Label("ustats by noobtuts.com\n")
        
        GUILayout.Label("Assets: ", EditorStyles.boldLabel)
        GUILayout.Label(" - Scripts: " + count_scripts)
        GUILayout.Label(" - Textures: " + count_textures)
        GUILayout.Label(" - Sounds: " + count_sounds)
        GUILayout.Label(" - Videos: " + count_videos)
        GUILayout.Label(" - Texts: " + count_texts + "\n")
        
        GUILayout.Label("Lines of Code: ", EditorStyles.boldLabel)
        GUILayout.Label(" - C#: " + loc_cs)
        GUILayout.Label(" - Javascript: " + loc_js)
        GUILayout.Label(" - Boo: " + loc_boo + "\n")
        
        GUILayout.Label("Time Spent:", EditorStyles.boldLabel)
        GUILayout.Label(" - Editing: " + format_seconds(time_editing))
        GUILayout.Label(" - Playing: " + format_seconds(time_playing) + "\n")
                
        GUILayout.EndVertical()
        
    static def recount_assets():
        # count file types
        exts = count_filetypes("Assets")
        count_scripts = sum(map({fmt | dict_get(exts, fmt)}, fmt_scripts))
        count_textures = sum(map({fmt | dict_get(exts, fmt)}, fmt_textures))
        count_sounds = sum(map({fmt | dict_get(exts, fmt)}, fmt_sounds))
        count_videos = sum(map({fmt | dict_get(exts, fmt)}, fmt_videos))
        count_texts = sum(map({fmt | dict_get(exts, fmt)}, fmt_texts))
        
        # count lines of code        
        exts = count_loc("Assets", fmt_scripts)
        loc_cs = dict_get(exts, ".cs")
        loc_js = dict_get(exts, ".js")
        loc_boo = dict_get(exts, ".boo")
    
    # count assets, repaint
    def OnProjectChange():
        recount_assets()
    
    # create our own Tick function that is being ticked while unity is active
    # -> not while not focused (unlike EditorApplication.update)
    # -> still while playing the game (unlike EditorWindow.Update)
    def Tick(deltaTime as single):
        # update times
        if EditorApplication.isPlaying and not EditorApplication.isPaused:
            time_playing += deltaTime
        else:
            time_editing += deltaTime
        
        # save once a minute
        lastsave += deltaTime
        if lastsave >= saveinterval:
            save(fname, time_editing, time_playing)
            lastsave = 0.0
    
    # Update that is called all the time, not just when window is visible
    # note: Time.time / deltaTime only works while playing, hence manual timing
    static time_last = EditorApplication.timeSinceStartup
    def UpdateAlways():
        elapsed = EditorApplication.timeSinceStartup - time_last
        if elapsed < 1: # is the window active? (updated >1/second?)
            Tick(elapsed)
        time_last = EditorApplication.timeSinceStartup
    
    # self created start function
    # (has to be called after starting unity and after starting game)
    def Start():
        OnProjectChange()
        time_editing, time_playing = load(fname)
        EditorApplication.update = Delegate.Combine(EditorApplication.update, UpdateAlways as EditorApplication.CallbackFunction) as EditorApplication.CallbackFunction
    
    # Update not needed, InspectorUpdate is enough
    # -> this function is only called while Unity is focused
    # -> not called when window is not active
    def OnInspectorUpdate():        
        # 'start' function
        if not initialized:
            initialized = true
            Start()
        # Repaint all the time so that time counters are refreshed too
        Repaint()
    