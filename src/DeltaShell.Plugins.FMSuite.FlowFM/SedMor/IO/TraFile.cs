using System;
using System.IO;
using System.Linq;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.SedMor.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.SedMor.IO
{
    public class TraFile
    {
        public TransportFormulation Load(string path)
        {            
            // From the Delft3d manual:
            // - The file may start with an arbitrary number of lines not containing the text IFORM. 
            // - Then a line starting with sediment transport formula number IFORM and containing text IFORM.
            // - Then an arbitrary number of lines starting with an asterisk (*) may follow.
            // - Then a line starting with the number sign (#) followed by a transport formula number
            //    optionally followed by text identifying the transport formula for the user. The next lines
            //    should contain the parameter values of the transport formula coefficients: one parameter
            //    value per line optionally followed by text identifying the parameter. There may be an
            //    arbitrary number of blocks starting with # in the file, but exactly one should correspond to
            //    the transport formula number IFORM specified above.

            TransportFormulation formulation = null;
            var foundIFORMLine = false;
            var inParameters = false;
            
            int iformNumber;
            var iformHeader = "";
            
            var parameterIndex = 0;
            var expectedParameters = -1;

            ModelPropertyGroup modelPropertyGroup = null;

            int lineNumber = 0;

            using (CultureUtils.SwitchToInvariantCulture())
            {
                foreach (var line in File.ReadLines(path).Select(l => l.TrimStart()))
                {
                    lineNumber++;

                    try
                    {
                        if (!foundIFORMLine && line.Contains("IFORM"))
                        {
                            iformNumber = int.Parse(line.Split(' ')[0]);
                            iformHeader = "#" + iformNumber.ToString();
                            formulation = new TransportFormulation(iformNumber);
                            modelPropertyGroup = TransportFormulation.TransportFormulationsSchema.ModelDefinitionCategory.First(
                                    md => md.Key == iformNumber.ToString()).Value;
                            expectedParameters = modelPropertyGroup.PropertyDefinitions.Count;
                            foundIFORMLine = true;
                        }

                        if (!foundIFORMLine)
                            continue;

                        if (!inParameters && line.StartsWith("*"))
                            continue; //skip line

                        if (!inParameters && line.StartsWith(iformHeader))
                        {
                            inParameters = true;
                            continue;
                        }

                        if (parameterIndex >= expectedParameters)
                            break; // done

                        if (inParameters)
                        {
                            var def = modelPropertyGroup.PropertyDefinitions[parameterIndex];
                            formulation.Properties.Add(def.FilePropertyName, new SedMorProperty(def, line.Split(' ')[0]));
                            parameterIndex++;
                        }
                    }
                    catch (Exception e)
                    {
                        throw new FormatException(string.Format("Error reading '{0}', line {1}: {2}", path, lineNumber,
                                                                e.Message), e);
                    }
                }
                return formulation;
            }
        }

        public void Save(string path, TransportFormulation formulation)
        {
            using (CultureUtils.SwitchToInvariantCulture())
            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("{0} Number of transport formula IFORM", formulation.IFormNumber);
                writer.WriteLine("*--------------------------------------------------------");
                writer.WriteLine("#{0} {1}", formulation.IFormNumber, formulation.Name);

                int index = 1;
                foreach (var prop in formulation.Properties)
                {
                    writer.WriteLine(" {0,-10} -Par {1}- {2}", prop.Value.GetValueAsString(), index,
                                     prop.Value.PropertyDefinition.Description);
                    index++;
                }
                writer.WriteLine("# End of specification of transport relation");
            }
        }
    }
}