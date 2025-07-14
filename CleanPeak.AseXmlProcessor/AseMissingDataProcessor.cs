using System.Xml;

namespace CleanPeak.AseXmlProcessor
{
    public static class AseMissingDataProcessor
    {
        public static XmlDocument GeneratePmd(string nmi, string toParticipantId, DateTime beginDate, DateTime endDate)
        {
            // Generating a random ID - This would be replaced with an identity value from the database
            var r = new Random();
            int id = r.Next(0, 10000);
            string pmdId = id.ToString("D8");

            var doc = XmlHelper.AseTemplate;
            doc.InnerXml = doc.InnerXml.Replace("{version}", Convert.ToString(ApplicationConfiguration.SchemaVersion));
            var aseNode = doc.SelectSingleNode("//Header")?.ParentNode;

            var transactions = doc.CreateElement("Transactions");

            aseNode.AppendChild(transactions);
            
            var tranDateXml = XmlHelper.ToXmlDateTime(DateTime.Now);

            XmlHelper.SetNode(ref doc, "From", ApplicationConfiguration.ParticipantId);
            XmlHelper.SetNode(ref doc, "To", toParticipantId);
            XmlHelper.SetNode(ref doc, "MessageID", $"CLEANPEAK-MSG-{pmdId}");
            XmlHelper.SetNode(ref doc, "TransactionGroup", "MTRD");
            XmlHelper.SetNode(ref doc, "Priority", "Medium");
            XmlHelper.SetNode(ref doc, "MessageDate", tranDateXml);

            var transaction = doc.CreateElement("Transaction");
            transaction.SetAttribute("transactionID", $"CLEANPEAK-TRAN-{pmdId}");
            transaction.SetAttribute("transactionDate", tranDateXml);

            var meterDataMissingNotification = doc.CreateElement("MeterDataMissingNotification");
            meterDataMissingNotification.SetAttribute("version", "r14");

            var missingMeterData = doc.CreateElement("MissingMeterData");
            missingMeterData.SetAttribute("xsi:type", "ase:ElectricityProvideMeterRequestData");
            missingMeterData.SetAttribute("version", "r17");

            var nmiElem = doc.CreateElement("NMI");
            nmiElem.SetAttribute("checksum", NmiHelper.NmiCheckSum(nmi));
            nmiElem.InnerText = nmi;
            missingMeterData.AppendChild(nmiElem);

            var nmiStandingData = doc.CreateElement("NMIStandingData");
            nmiStandingData.SetAttribute("xsi:type", "ase:ElectricityStandingData");
            nmiStandingData.SetAttribute("version", "r43");

            var roleAssignments = doc.CreateElement("RoleAssignments");
            var roleAssignment = doc.CreateElement("RoleAssignment");
            var role = doc.CreateElement("Role");
            role.InnerText = "FRMP";
            roleAssignment.AppendChild(role);
            roleAssignments.AppendChild(roleAssignment);
            nmiStandingData.AppendChild(roleAssignments);
            missingMeterData.AppendChild(nmiStandingData);

            var requestPeriod = doc.CreateElement("RequestPeriod");
            var beginElem = doc.CreateElement("BeginDate");
            beginElem.InnerText = beginDate.ToString("yyyy-MM-dd");
            var endElem = doc.CreateElement("EndDate");
            endElem.InnerText = endDate.ToString("yyyy-MM-dd");
            requestPeriod.AppendChild(beginElem);
            requestPeriod.AppendChild(endElem);
            missingMeterData.AppendChild(requestPeriod);

            meterDataMissingNotification.AppendChild(missingMeterData);
            transaction.AppendChild(meterDataMissingNotification);
            transactions.AppendChild(transaction);

            return doc;
        }
    }
}
