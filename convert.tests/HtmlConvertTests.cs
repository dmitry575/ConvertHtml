using NUnit.Framework;
using System.IO;
using System.Linq;

namespace convert.tests
{
    public class HtmlConvertTests
    {
        [Test]
        public void ConvertSuccess()
        {
            var htmlConvert = new HtmlConvert("./DataConvert", "./results", "./jsons");

            htmlConvert.Convert("*.html");

            var htmlUnConvert = new HtmlUnConvert("./results", "./UnConvert", "./jsons");

            htmlUnConvert.UnConvert("data.json", "");

            var content = File.ReadAllText("UnConvert/tags_17_.html");

            Assert.AreEqual(CountSubstring(content, "<small>"), 3);
            Assert.AreEqual(CountSubstring(content, "</small>"), 3);
            Assert.AreEqual(CountSubstring(content, "-121---"), 3);
            
            Assert.IsTrue(content.Contains("<h3>End of service</h3>"));

        }

        private int CountSubstring(string content, string substr)
        {
            return content.Select((_, i) => content.Skip(i).Take(substr.Length)).Count(s => s.SequenceEqual(substr));
        }
    }
}
