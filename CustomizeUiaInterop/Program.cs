using System;
using System.IO;

namespace CustomizeUiaInterop
{
    internal class Program
    {
        private static void ShowHelp()
        {
            Console.WriteLine("CustomizeUiaInterop <input> <output>");
        }

        private static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                ShowHelp();
                return 1;
            }

            var inputFilePath = args[0];
            var outputFilePath = args[1];

            try
            {
                // Read all the text
                var contents = File.ReadAllText(inputFilePath);

                var updated = contents.Replace(
@"  .method public hidebysig newslot abstract virtual 
          instance void  RegisterPattern([in] valuetype Interop.UIAutomationCore.UIAutomationPatternInfo& pattern,
                                         [out] int32& pPatternId,
                                         [out] int32& pPatternAvailablePropertyId,
                                         [in] uint32 propertyIdCount,
                                         [out] int32& pPropertyIds,
                                         [in] uint32 eventIdCount,
                                         [out] int32& pEventIds) runtime managed internalcall",
@"  .method public hidebysig newslot abstract virtual 
          instance void  RegisterPattern([in] valuetype Interop.UIAutomationCore.UIAutomationPatternInfo& pattern,
                                         [out] int32& pPatternId,
                                         [out] int32& pPatternAvailablePropertyId,
                                         [in] uint32 propertyIdCount,
                                         [out] int32[] marshal([+3]) pPropertyIds,
                                         [in] uint32 eventIdCount,
                                         [out] int32[] marshal([+5]) pEventIds) runtime managed internalcall");

                updated = updated.Replace(
@"  .method public hidebysig newslot virtual 
          instance void  RegisterPattern([in] valuetype Interop.UIAutomationCore.UIAutomationPatternInfo& pattern,
                                         [out] int32& pPatternId,
                                         [out] int32& pPatternAvailablePropertyId,
                                         [in] uint32 propertyIdCount,
                                         [out] int32& pPropertyIds,
                                         [in] uint32 eventIdCount,
                                         [out] int32& pEventIds) runtime managed internalcall",
@"  .method public hidebysig newslot virtual 
          instance void  RegisterPattern([in] valuetype Interop.UIAutomationCore.UIAutomationPatternInfo& pattern,
                                         [out] int32& pPatternId,
                                         [out] int32& pPatternAvailablePropertyId,
                                         [in] uint32 propertyIdCount,
                                         [out] int32[] marshal([+3]) pPropertyIds,
                                         [in] uint32 eventIdCount,
                                         [out] int32[] marshal([+5]) pEventIds) runtime managed internalcall");

                updated = updated.Replace(
@"  .method public hidebysig newslot abstract virtual 
          instance void  CallMethod([in] uint32 index,
                                    [in] valuetype Interop.UIAutomationCore.UIAutomationParameter& pParams,
                                    [in] uint32 cParams) runtime managed internalcall",
@"  .method public hidebysig newslot abstract virtual 
          instance void  CallMethod([in] uint32 index,
                                    [in] valuetype Interop.UIAutomationCore.UIAutomationParameter[] marshal([+2]) pParams,
                                    [in] uint32 cParams) runtime managed internalcall");

                updated = updated.Replace(
@"  .method public hidebysig newslot abstract virtual 
          instance void  Dispatch([in] object  marshal( iunknown ) pTarget,
                                  [in] uint32 index,
                                  [in] valuetype Interop.UIAutomationCore.UIAutomationParameter& pParams,
                                  [in] uint32 cParams) runtime managed internalcall",
@"  .method public hidebysig newslot abstract virtual 
          instance void  Dispatch([in] object  marshal( iunknown ) pTarget,
                                  [in] uint32 index,
                                  [in] valuetype Interop.UIAutomationCore.UIAutomationParameter[] marshal([+3]) pParams,
                                  [in] uint32 cParams) runtime managed internalcall");

                updated = updated.Replace(
@".class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationPropertyInfo
       extends [mscorlib]System.ValueType
{
  .pack 4",
@".class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationPropertyInfo
       extends [mscorlib]System.ValueType
{
  .pack 0");

                updated = updated.Replace(
@".class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationEventInfo
       extends [mscorlib]System.ValueType
{
  .pack 4",
@".class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationEventInfo
       extends [mscorlib]System.ValueType
{
  .pack 0");

                updated = updated.Replace(
@".class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationPatternInfo
       extends [mscorlib]System.ValueType
{
  .pack 4",
@".class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationPatternInfo
       extends [mscorlib]System.ValueType
{
  .pack 0");

                updated = updated.Replace(
@".class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationParameter
       extends [mscorlib]System.ValueType
{
  .pack 4",
@".class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationParameter
       extends [mscorlib]System.ValueType
{
  .pack 0");

                updated = updated.Replace(
@".class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationMethodInfo
       extends [mscorlib]System.ValueType
{
  .pack 4",
@".class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationMethodInfo
       extends [mscorlib]System.ValueType
{
  .pack 0");

                // Write all the text
                File.WriteAllText(outputFilePath, updated);
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception:");
                Console.WriteLine(e.ToString());
                return 1;
            }

            return 0;
        }
    }
}