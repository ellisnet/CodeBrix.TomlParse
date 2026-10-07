using System.Linq;
using CodeBrix.TomlParse.Serialization.Internal;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public class PooledArrayBuilderTests
{
    [Fact]
    public void ToArrayAndReturn_EmptyBuilder_ReturnsEmptyArrayAndResetsCount()
    {
        var builder = new PooledArrayBuilder<int>(initialCapacity: 4);

        var result = builder.ToArrayAndReturn();

        result.Should().BeEmpty();
        builder.Count.Should().Be(0);
    }

    [Fact]
    public void Add_GrowingBeyondInitialCapacity_PreservesOrderAndCount()
    {
        var builder = new PooledArrayBuilder<int>(initialCapacity: 2);

        for (var value = 1; value <= 33; value++)
        {
            builder.Add(value);
            builder.Count.Should().Be(value);
        }

        var result = builder.ToArrayAndReturn();

        result.Should().Equal(Enumerable.Range(1, 33).ToArray());
        builder.Count.Should().Be(0);
    }

    [Fact]
    public void ToArrayAndReturn_AfterReuse_ReturnsOnlyNewValues()
    {
        var builder = new PooledArrayBuilder<int>(initialCapacity: 1);
        builder.Add(1);
        builder.Add(2);

        var first = builder.ToArrayAndReturn();

        builder.Add(3);
        builder.Add(4);
        var second = builder.ToArrayAndReturn();

        first.Should().Equal(new[] { 1, 2 });
        second.Should().Equal(new[] { 3, 4 });
    }

    [Fact]
    public void Add_AfterDispose_UsesFreshStorage()
    {
        var builder = new PooledArrayBuilder<string>(initialCapacity: 1);
        builder.Add("alpha");
        builder.Add("beta");

        builder.Dispose();
        builder.Add("gamma");

        var result = builder.ToArrayAndReturn();

        builder.Count.Should().Be(0);
        result.Should().Equal(new[] { "gamma" });
    }
}
