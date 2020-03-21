using System.Threading.Tasks;
using Xunit;
using Amazon.Lambda.TestUtilities;
using Pecuniary.TimeSeries;

namespace HelloWorld.Tests
{
  public class FunctionTest
  {
    [Fact]
    public async Task TestHelloWorldFunctionHandler()
    {
        var context = new TestLambdaContext();
        
        var function = new Function();
        await function.FunctionHandler(context);

        //Assert.NotNull(response);
        //Assert.Equal(expectedResponse.Body, response.Body);
        //Assert.Equal(expectedResponse.Headers, response.Headers);
        //Assert.Equal(expectedResponse.StatusCode, response.StatusCode);
    }
  }
}