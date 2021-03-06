using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace WebOptimizer.Sass.Test
{
    public class CompilerTest
    {
        [Fact]
        public async Task Compile_Success()
        {
            var processor = new Compiler();
            var pipeline = new Mock<IAssetPipeline>().SetupAllProperties();
            var context = new Mock<IAssetContext>().SetupAllProperties();
            var asset = new Mock<IAsset>().SetupAllProperties();
            var env = new Mock<IHostingEnvironment>();
            var fileProvider = new Mock<IFileProvider>();

            context.Object.Content = new Dictionary<string, byte[]> {
                { "/file.css", "$bg: blue; div {background: $bg}".AsByteArray() },
            };

            context.Setup(s => s.HttpContext.RequestServices.GetService(typeof(IHostingEnvironment)))
                   .Returns(env.Object);

            string temp = Path.GetTempPath();
            var inputFile = new PhysicalFileInfo(new FileInfo("site.css"));

            context.SetupGet(s => s.Asset)
                          .Returns(asset.Object);

            env.SetupGet(e => e.WebRootFileProvider)
                 .Returns(fileProvider.Object);

            fileProvider.Setup(f => f.GetFileInfo(It.IsAny<string>()))
                   .Returns(inputFile);

            await processor.ExecuteAsync(context.Object);
            var result = context.Object.Content.First().Value;
            Assert.Equal("div {\n  background: blue; }\n", result.AsString());
        }
    }
}
