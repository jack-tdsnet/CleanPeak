using System.Xml;

namespace CleanPeak.AseXmlProcessor
{
    public static class AseAckProcessor
    {
        public static XmlDocument GenerateAck(string inputAseXml)
        {
            if (string.IsNullOrWhiteSpace(inputAseXml))
            {
                throw new ArgumentException("Input XML cannot be null or empty.", nameof(inputAseXml));
            }

            try
            {
                var aseDoc = new XmlDocument();
                aseDoc.LoadXml(inputAseXml);

                var schemaVersion = XmlHelper.SelectSchemaVersion(aseDoc);

                var header = (XmlElement)aseDoc.SelectSingleNode("*/Header");
                var fromName = XmlHelper.GetElement(header, "From");
                var toName = XmlHelper.GetElement(header, "To");
                var messageId = XmlHelper.GetElement(header, "MessageID");
                var messageDate = XmlHelper.FromXmlDateTime(XmlHelper.GetElement(header, "MessageDate"));
                var tranGroup = XmlHelper.GetElement(header, "TransactionGroup");
                var priority = XmlHelper.GetElement(header, "Priority");

                // Generating a random ID - This would be replaced with an identity value from the database
                var r = new Random();
                int id = r.Next(0, 10000);
                string ackId = id.ToString("D8");

                var aseAck = XmlHelper.AseTemplate;
                aseAck.InnerXml = aseAck.InnerXml.Replace("{version}", Convert.ToString(schemaVersion));
                var aseNode = aseAck.SelectSingleNode("//Header")?.ParentNode;

                var list = aseAck.CreateElement("Acknowledgements");

                aseNode.AppendChild(list);

                var ackDateXml = XmlHelper.ToXmlDateTime(DateTime.Now);

                XmlHelper.SetNode(ref aseAck, "From", toName);
                XmlHelper.SetNode(ref aseAck, "To", fromName);
                XmlHelper.SetNode(ref aseAck, "MessageID", $"CLEANPEAK_ACKM_{ackId}");
                XmlHelper.SetNode(ref aseAck, "TransactionGroup", tranGroup);
                XmlHelper.SetNode(ref aseAck, "Priority", priority);
                XmlHelper.SetNode(ref aseAck, "MessageDate", ackDateXml);

                var messageAck = aseAck.CreateElement("MessageAcknowledgement");

                list.AppendChild(messageAck);

                messageAck.SetAttribute("status", "Accept");
                messageAck.SetAttribute("receiptID", $"CLEANPEAK_ACKR_{ackId}");
                messageAck.SetAttribute("receiptDate", ackDateXml);
                messageAck.SetAttribute("initiatingMessageID", messageId);

                foreach (var tran in aseDoc.SelectNodes("//Transaction"))
                {
                    var tranAck = aseAck.CreateElement("TransactionAcknowledgement");

                    list.AppendChild(tranAck);

                    tranAck.SetAttribute("status", "Accept");
                    tranAck.SetAttribute("receiptID", $"CLEANPEAK_ACKT_{ackId}");
                    tranAck.SetAttribute("receiptDate", ackDateXml);
                    tranAck.SetAttribute("initiatingTransactionID", ((XmlElement)tran).GetAttribute("transactionID"));
                }

                return aseAck;
            }
            catch (XmlException ex)
            {
                throw new InvalidOperationException("Failed to parse the input XML.", ex);
            }
        }
    }
}
