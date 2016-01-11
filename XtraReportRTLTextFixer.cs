using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System;

namespace RDF.Reports.DevX
{
    /// <summary>
    /// Provide methods for fixing RTL text direction problems by inject Right-To-Left mark to report elements
    /// </summary>
    public static class XtraReportRTLTextFixer
    {
        /// <summary>
        /// Left-To-Right Mark character
        /// </summary>
        public const char LRM = '\u200E';
        /// <summary>
        /// Right-To-Left Mark character
        /// </summary>
        public const char RLM = '\u200F';
        /// <summary>
        /// Right-To-LeftEmbedding character
        /// </summary>
        public const char RLE = '\u202B';
        /// <summary>
        /// Pop Directional Formatting character
        /// </summary>
        public const char PDF = '\u202C';
        /// <summary>        
        /// Left-To-Right Embedding
        /// </summary>
        public const char LRE = '\u202A';
        /// <summary>
        /// Left-To-Right override
        /// </summary>
        public const char LRO = '\u202D';
        /// <summary>
        /// Right-To-Left override
        /// </summary>
        public const char RLO = '\u202E';

        /// <summary>
        /// Fix all cells (of either XRLabel and XRTableCell) by injeting Right-To-Left mark (U+200F)
        ///  to the end of their Text proeprty. SubReports are iterated automatically, too.
        /// </summary>
        /// <param name="report">The report to fix its RTL direction alignment</param>
        public static void FixRTLText(this XtraReport report)
        {
            report.BeforePrint += (sender, args) =>
            {
                IterateCells(report);

                // Iterate all XRSubReport
                var subs = report.AllControls<XRSubreport>();
                if (subs.Any())
                {
                    // Iterate all subreports and fix them
                    foreach (var subreoprt in subs)
                        subreoprt.ReportSource.FixRTLText();
                }
            };
        }

        /// <summary>
        /// Iterates alls XRTableCell and XRLabel cells in the given report
        /// </summary>
        /// <param name="report">The report to fix its RTL direction alignment</param>
        private static void IterateCells(XtraReport report)
        {
            // Iterate all XRTableCells and fix them
            MakeCellsRightToLeft(report, report.AllControls<XRTableCell>());

            // Iterate all XRLabels and fix them
            MakeCellsRightToLeft(report, report.AllControls<XRLabel>());
        }

        /// <summary>
        /// Fixes RTL alignment direction for the given cells in the given report
        /// </summary>
        /// <param name="report">The report to fix its RTL direction alignment</param>
        /// <param name="cells">Cells to intercept their Text proeprty for a possible RTL fix</param>
        private static void MakeCellsRightToLeft(XtraReport report, IEnumerable<XRLabel> cells)
        {
            foreach (var cell in cells)
            {
                // Searching for cells that have a binding on their Text property
                var binding = cell.DataBindings.FirstOrDefault(x => x.PropertyName == "Text");

                if (binding != null)
                {
                    // In order to be able to unregister from the event we have to have
                    // a reference to the handler
                    PrintEventHandler beforePrintDelegate = null;

                    beforePrintDelegate = (_s, _e) =>
                    {
                        var value = report.GetCurrentColumnValue(binding.DataMember);

                        if (report.Parameters.Cast<Parameter>().Any(x => x.Name == binding.DataMember))
                        {
                            // a parameter is bound to this control
                            cell.Text = FixRTL(cell.Text);
                        }
                        else
                        {
                            // Examine the data type of the obtained value if it's not string
                            // we unregister from the BeforePrint event not to get future notifications
                            if (value is string)
                            {
                                // Adds a Right-To-Left MARK (U+200F) to the end of the string
                                (_s as XRLabel).Text = FixRTL(value.ToString());
                            }
                            else
                            {
                                // Unregisters from the BeforePrint
                                cell.BeforePrint -= beforePrintDelegate;
                            }
                        }
                    };

                    // Registering to the BeforePrint event to intercept the string values
                    cell.BeforePrint += beforePrintDelegate;
                }
                else
                {
                    // If the cell has no binding to any source so just add RLM character
                    // to the end of its Text property
                    cell.Text = FixRTL(cell.Text);
                }
            }
        }

        /// <summary>
        /// Tries to fix the direction of the RTL text by wrapping the text with appropriate direction characters 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string FixRTL(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            return RLE + input + PDF + RLM;
        }
    }
}
