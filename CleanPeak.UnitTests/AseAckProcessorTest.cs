using CleanPeak.AseXmlProcessor;

namespace CleanPeak.UnitTests
{
    public class AseAckProcessorTest
    {
        private const string TestFilesFolder = "TestFiles";

        [Fact]
        public void ProcessAck_ValidXmlFiles_ShouldReturnExpectedResults()
        {
            Assert.True(Directory.Exists(TestFilesFolder), $"Test folder '{TestFilesFolder}' does not exist.");

            var aseFiles = Directory.GetFiles(TestFilesFolder, "*.xml");
            Assert.NotEmpty(aseFiles);

            foreach (var file in aseFiles)
            {
                var inputXml = File.ReadAllText(file);

                var aseAck = AseAckProcessor.GenerateAck(inputXml);

                Assert.NotNull(aseAck);
            }
        }

        [Fact]
        public void GeneratePmd_ShouldProduceValidXml()
        {
            string nmi = "0123456789";
            string toParticipantId = "TESTPARTID";
            DateTime beginDate = new DateTime(2025, 1, 1);
            DateTime endDate = new DateTime(2025, 1, 15);

            var pmd = AseMissingDataProcessor.GeneratePmd(nmi, toParticipantId, beginDate, endDate);

            Assert.NotNull(pmd);

            var transactionsNode = pmd.SelectSingleNode("//Transactions");
            Assert.NotNull(transactionsNode);

            var transactionNode = transactionsNode.SelectSingleNode("Transaction");
            Assert.NotNull(transactionNode);

            var nmiNode = transactionNode.SelectSingleNode(".//NMI");
            Assert.NotNull(nmiNode);
            Assert.Equal(nmi, nmiNode.InnerText);
            Assert.Equal(NmiHelper.NmiCheckSum(nmi), nmiNode.Attributes["checksum"].Value);
        }
    }
}
