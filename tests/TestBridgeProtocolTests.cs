using NUnit.Framework;
using ProceduralMaze.Maze;
using ProceduralMaze.Testing;
using System.Text;

namespace ProceduralMaze.Tests;

/// <summary>
/// Covers the test bridge's wire format — the query/command parsing and JSON writing that
/// Playwright talks to (see docs/TEST_BRIDGE.md).
///
/// Worth real tests because the parsers are hand-rolled: the web build is trimmed, so a
/// reflection-based serializer would break in the browser only. That trade means the parsing
/// is ours to get right, and a bug here surfaces as a Playwright test that mysteriously sees
/// the wrong state.
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.All)]
public class TestBridgeProtocolTests
{
    #region Query string

    [TestCase("?seed=42", "seed", "42")]
    [TestCase("seed=42", "seed", "42")]
    [TestCase("?a=1&seed=42&b=2", "seed", "42")]
    [TestCase("?seed=42&algorithm=prims", "algorithm", "prims")]
    [TestCase("?seed=-7", "seed", "-7")]
    public void GetParam_ExtractsValue(string query, string name, string expected)
    {
        Assert.That(TestBridgeProtocol.GetParam(query, name), Is.EqualTo(expected));
    }

    [TestCase("", "seed")]
    [TestCase("?other=1", "seed")]
    [TestCase("?seedling=1", "seed")]      // must not prefix-match a different key
    [TestCase("?xseed=1", "seed")]         // nor suffix-match
    [TestCase("?seed", "seed")]            // no '=' means no value
    public void GetParam_ReturnsNullWhenAbsent(string query, string name)
    {
        Assert.That(TestBridgeProtocol.GetParam(query, name), Is.Null);
    }

    [Test]
    public void GetParam_HandlesNullQuery()
    {
        Assert.That(TestBridgeProtocol.GetParam(null, "seed"), Is.Null);
    }

    [Test]
    public void GetParam_UrlDecodesValue()
    {
        Assert.That(TestBridgeProtocol.GetParam("?scene=comparison%20dashboard", "scene"),
            Is.EqualTo("comparison dashboard"));
    }

    [TestCase("?seed=42", 42)]
    [TestCase("?seed=-7", -7)]
    [TestCase("?seed=0", 0)]
    public void GetIntParam_ParsesInteger(string query, int expected)
    {
        Assert.That(TestBridgeProtocol.GetIntParam(query, "seed"), Is.EqualTo(expected));
    }

    [TestCase("?seed=abc")]
    [TestCase("?seed=1.5")]
    [TestCase("?seed=")]
    [TestCase("?other=1")]
    public void GetIntParam_ReturnsNullWhenNotAnInteger(string query)
    {
        Assert.That(TestBridgeProtocol.GetIntParam(query, "seed"), Is.Null);
    }

    #endregion

    #region Command JSON

    [Test]
    public void GetJsonString_ExtractsValue()
    {
        const string json = """{"cmd":"generate","algorithm":"prims"}""";
        Assert.Multiple(() =>
        {
            Assert.That(TestBridgeProtocol.GetJsonString(json, "cmd"), Is.EqualTo("generate"));
            Assert.That(TestBridgeProtocol.GetJsonString(json, "algorithm"), Is.EqualTo("prims"));
            Assert.That(TestBridgeProtocol.GetJsonString(json, "missing"), Is.Null);
        });
    }

    [Test]
    public void GetJsonString_TolerantOfWhitespaceAfterColon()
    {
        Assert.That(TestBridgeProtocol.GetJsonString("""{"cmd" : "goto"}""", "cmd"), Is.EqualTo("goto"));
    }

    [Test]
    public void GetJsonString_UnescapesEscapeSequences()
    {
        Assert.That(TestBridgeProtocol.GetJsonString("""{"m":"a\"b"}""", "m"), Is.EqualTo("a\"b"));
        Assert.That(TestBridgeProtocol.GetJsonString("""{"m":"a\nb"}""", "m"), Is.EqualTo("a\nb"));
    }

    [Test]
    public void GetJsonString_ReturnsNullForUnterminatedString()
    {
        Assert.That(TestBridgeProtocol.GetJsonString("""{"cmd":"generate""", "cmd"), Is.Null);
    }

    [Test]
    public void GetJsonString_ReturnsNullWhenValueIsNotAString()
    {
        Assert.That(TestBridgeProtocol.GetJsonString("""{"seed":42}""", "seed"), Is.Null);
    }

    [TestCase("""{"seed":42}""", 42)]
    [TestCase("""{"seed":-7}""", -7)]
    [TestCase("""{"seed": 42}""", 42)]
    [TestCase("""{"cmd":"generate","seed":123,"x":10}""", 123)]
    public void GetJsonInt_ExtractsValue(string json, int expected)
    {
        Assert.That(TestBridgeProtocol.GetJsonInt(json, "seed"), Is.EqualTo(expected));
    }

    [TestCase("""{"seed":"42"}""")]   // string, not a number
    [TestCase("""{"other":1}""")]
    public void GetJsonInt_ReturnsNullWhenNotAnInteger(string json)
    {
        Assert.That(TestBridgeProtocol.GetJsonInt(json, "seed"), Is.Null);
    }

    [Test]
    public void GetJsonBool_ExtractsValue()
    {
        Assert.Multiple(() =>
        {
            Assert.That(TestBridgeProtocol.GetJsonBool("""{"value":true}""", "value"), Is.True);
            Assert.That(TestBridgeProtocol.GetJsonBool("""{"value":false}""", "value"), Is.False);
            Assert.That(TestBridgeProtocol.GetJsonBool("""{"value":1}""", "value"), Is.Null);
            Assert.That(TestBridgeProtocol.GetJsonBool("""{"other":true}""", "value"), Is.Null);
        });
    }

    [Test]
    public void Parsers_HandleEmptyAndNullJson()
    {
        Assert.Multiple(() =>
        {
            Assert.That(TestBridgeProtocol.GetJsonString(null, "cmd"), Is.Null);
            Assert.That(TestBridgeProtocol.GetJsonInt("", "seed"), Is.Null);
            Assert.That(TestBridgeProtocol.GetJsonBool("{}", "value"), Is.Null);
        });
    }

    #endregion

    #region Mappings

    [TestCase("backtracker", Algorithm.RecursiveBacktrackerAlgorithm)]
    [TestCase("recursivebacktracker", Algorithm.RecursiveBacktrackerAlgorithm)]
    [TestCase("growingtree", Algorithm.GrowingTreeAlgorithm)]
    [TestCase("binarytree", Algorithm.BinaryTreeAlgorithm)]
    [TestCase("prims", Algorithm.PrimsAlgorithm)]
    [TestCase("PRIMS", Algorithm.PrimsAlgorithm)]
    [TestCase("Prims", Algorithm.PrimsAlgorithm)]
    public void ParseAlgorithm_MapsKnownNames(string name, Algorithm expected)
    {
        Assert.That(TestBridgeProtocol.ParseAlgorithm(name), Is.EqualTo(expected));
    }

    [TestCase("nonsense")]
    [TestCase("")]
    [TestCase(null)]
    public void ParseAlgorithm_ReturnsNullForUnknown(string? name)
    {
        // Null matters: the caller leaves the existing setting alone rather than guessing.
        Assert.That(TestBridgeProtocol.ParseAlgorithm(name), Is.Null);
    }

    [Test]
    public void ParseAlgorithm_CoversEveryAlgorithmTheAppSupports()
    {
        // If someone adds an algorithm, the URL/command surface should not silently omit it.
        var mappable = new[] { "backtracker", "growingtree", "binarytree", "prims" }
            .Select(TestBridgeProtocol.ParseAlgorithm)
            .Where(a => a is not null)
            .Select(a => a!.Value)
            .ToHashSet();

        var supported = Enum.GetValues<Algorithm>().Where(a => a != Algorithm.None).ToHashSet();

        Assert.That(mappable, Is.EquivalentTo(supported),
            "Every Algorithm except None should be reachable from a URL/command name.");
    }

    [TestCase("maze", "res://scenes/maze.tscn")]
    [TestCase("menu", "res://scenes/menu.tscn")]
    [TestCase("comparison", "res://scenes/comparison_dashboard.tscn")]
    [TestCase("loader", "res://scenes/maze_loader.tscn")]
    [TestCase("about", "res://scenes/about.tscn")]
    public void ResolveScenePath_MapsAliases(string alias, string expected)
    {
        Assert.That(TestBridgeProtocol.ResolveScenePath(alias), Is.EqualTo(expected));
    }

    [Test]
    public void ResolveScenePath_ReturnsNullForUnknownAlias()
    {
        Assert.That(TestBridgeProtocol.ResolveScenePath("nope"), Is.Null);
    }

    #endregion

    #region JSON writing

    [Test]
    public void AppendString_EscapesJsonSpecialCharacters()
    {
        var sb = new StringBuilder();
        TestBridgeProtocol.AppendString(sb, "msg", "he said \"hi\"\nand\\left\t");
        Assert.That(sb.ToString(), Is.EqualTo("\"msg\":\"he said \\\"hi\\\"\\nand\\\\left\\t\""));
    }

    [Test]
    public void AppendString_EscapesControlCharacters()
    {
        var sb = new StringBuilder();
        TestBridgeProtocol.AppendString(sb, "m", "\u0001");
        Assert.That(sb.ToString(), Is.EqualTo("\"m\":\"\\u0001\""));
    }

    [Test]
    public void AppendString_HandlesNullValue()
    {
        var sb = new StringBuilder();
        TestBridgeProtocol.AppendString(sb, "m", null);
        Assert.That(sb.ToString(), Is.EqualTo("\"m\":\"\""));
    }

    [Test]
    public void AppendInt_AndAppendBool_WriteExpectedJson()
    {
        var sb = new StringBuilder();
        TestBridgeProtocol.AppendInt(sb, "n", -12);
        sb.Append(',');
        TestBridgeProtocol.AppendBool(sb, "b", true);
        Assert.That(sb.ToString(), Is.EqualTo("\"n\":-12,\"b\":true"));
    }

    [Test]
    public void WrittenJson_IsReadableByTheParsers()
    {
        // Round-trip: whatever the bridge publishes must be parseable by the same protocol,
        // which is the closest thing to an end-to-end check available without the engine.
        var sb = new StringBuilder();
        sb.Append('{');
        TestBridgeProtocol.AppendString(sb, "cmd", "generate");
        sb.Append(',');
        TestBridgeProtocol.AppendInt(sb, "seed", 20260725);
        sb.Append(',');
        TestBridgeProtocol.AppendBool(sb, "value", false);
        sb.Append('}');
        var json = sb.ToString();

        Assert.Multiple(() =>
        {
            Assert.That(TestBridgeProtocol.GetJsonString(json, "cmd"), Is.EqualTo("generate"));
            Assert.That(TestBridgeProtocol.GetJsonInt(json, "seed"), Is.EqualTo(20260725));
            Assert.That(TestBridgeProtocol.GetJsonBool(json, "value"), Is.False);
        });
    }

    [Test]
    public void WrittenJson_SurvivesEscapedContentRoundTrip()
    {
        // An error message containing quotes/newlines is the realistic case: lastError is
        // published this way, and a broken escape would corrupt the whole state object.
        var sb = new StringBuilder();
        sb.Append('{');
        TestBridgeProtocol.AppendString(sb, "lastError", "Bad \"input\"\nline2");
        sb.Append('}');

        Assert.That(TestBridgeProtocol.GetJsonString(sb.ToString(), "lastError"),
            Is.EqualTo("Bad \"input\"\nline2"));
    }

    #endregion
}
