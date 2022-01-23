using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace convert.tests
{
    public class Tests
    {
        private string _findSeparate = "-121---([0-9]+)-";

        [SetUp]
        public void Setup()
        {
        }

        [TestCase(" - 121-- -844123- sasdf sdf ", " -121---844123- sasdf sdf ")]
        [TestCase("df -121 --- 844123 - sasdf sdf ", "df -121---844123- sasdf sdf ")]
        [TestCase("df - 121 --- 844123- sasdf sdf ", "df -121---844123- sasdf sdf ")]
        [TestCase("df -121. --- 844123 - sasdf sdf ", "df -121---844123- sasdf sdf ")]
        [TestCase("df -121---882193... sasdf sdf ", "df -121---882193-.. sasdf sdf ")]
        
        public void GetAfterTranslate(string t1, string res)
        {
            var htmlUnConvert = new HtmlUnConvert("./", "./", "./");


            var result = htmlUnConvert.GetAfterTranslate(t1);
            Assert.AreEqual(result, res);
        }

        [TestCase("Data/comments_0_.ru.en.txt", "Data/comments_0_.txt")]
        public void GetAfterTranslateFile(string filename,string fileBeforeTranslate)
        {
            var htmlUnConvert = new HtmlUnConvert("./", "./", "./");

            var result = htmlUnConvert.GetAfterTranslate(File.ReadAllText(filename));
            var collections = Regex.Split(result, _findSeparate, RegexOptions.Singleline)
                .Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            var collectionsReal = Regex.Split(File.ReadAllText(fileBeforeTranslate), _findSeparate, RegexOptions.Singleline)
                .Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            for (var i = 1; i < collectionsReal.Count; i+=2)
            {
                Assert.AreEqual(collections[i],collectionsReal[i],"i: {0}",i);
            }

            Assert.AreEqual(collections.Count, 400);
        }
    }
}