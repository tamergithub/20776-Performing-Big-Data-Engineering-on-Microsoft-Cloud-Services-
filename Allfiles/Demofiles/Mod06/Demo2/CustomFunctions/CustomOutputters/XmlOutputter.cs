using Microsoft.Analytics.Interfaces;
using System;
using System.IO;
using System.Text;

namespace CustomOutputters
{
    // Output data in a simple XML format
    [SqlUserDefinedOutputter(AtomicFileProcessing = false)]
    public class SimpleXMLOutputter : IOutputter
    {
        private string xmlDocType = String.Empty;
        private Stream outputWriter;

        // The constructor specifies the XML tags to use to wrap the data (defaults to <Data><Row>...</Row><Row>...</Row>...</Data>)
        public SimpleXMLOutputter(string xmlDocType = "Data")
        {
            this.xmlDocType = xmlDocType;
            this.outputWriter = null;
        }

        public override void Output(IRow input, IUnstructuredWriter output)
        {
            // If this is the first row, then get the stream output set up by the U-SQL runtime
            if (this.outputWriter == null)
            {
                this.outputWriter = output.BaseStream;
            }

            // Get the schema of the row
            var columnSchema = input.Schema;

            // Iterate through the columns in the row and convert the data to XML encoded strings
            StringBuilder rowData = new StringBuilder($@"<{xmlDocType}>");
            foreach (var column in columnSchema)
            {
                rowData.Append($@"<{column.Name}>");

                // This outputter currently only recognizes int, double, string, and DateTime data.
                if (column.Type == typeof(int))
                    rowData.Append($@"{input.Get<int>(column.Name)}");
                if (column.Type == typeof(double))
                    rowData.Append($@"{input.Get<double>(column.Name)}");
                if (column.Type == typeof(string))
                    rowData.Append($@"{input.Get<string>(column.Name)}");
                if (column.Type == typeof(DateTime))
                    rowData.Append($@"{input.Get<DateTime>(column.Name)}");

                rowData.Append($@"</{column.Name}>");
            }
            rowData.Append($@"</{xmlDocType}>");
            rowData.Append(Environment.NewLine);

            // Send the XML encoded string to the output stream
            string data = rowData.ToString();
            this.outputWriter.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);
        }

        // Flush any remaining buffered output, and then close the destination
        public override void Close()
        {
            this.outputWriter.Flush();
            this.outputWriter.Close();
        }
    }
}