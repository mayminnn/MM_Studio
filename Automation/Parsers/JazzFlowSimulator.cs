using Automation.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Automation.Parsers
{
    public class JazzFlowSimulator
    {
        private readonly Dictionary<string, JazzProgram> _subFlows;

        public JazzFlowSimulator(Dictionary<string, JazzProgram> subFlows = null)
        {
            _subFlows = subFlows ?? new Dictionary<string, JazzProgram>(StringComparer.OrdinalIgnoreCase);
        }

        public JazzSiteResult Run(JazzProgram program, Dictionary<string, List<bool>> outcomes)
        {
            var state = new ExecutionState();
            var consumed = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            RunSteps(program.Steps, outcomes, consumed, state);

            return new JazzSiteResult { Bin = state.Bin, Result = state.Result };
        }

        private class ExecutionState
        {
            public string Bin;
            public string Result;
            public string Binport;
        }

        private void RunSteps(List<JazzStepDef> steps, Dictionary<string, List<bool>> outcomes, Dictionary<string, int> consumed, ExecutionState state)
        {
            int index = 0;

            while (index < steps.Count)
            {
                JazzStepDef step = steps[index];
                string cmd = step.Command?.Trim().ToLowerInvariant() ?? "";

                if (cmd == "return")
                {
                    ApplyUnconditional(step, state);
                    return;
                }

                if (cmd == "call")
                {
                    if (!_subFlows.TryGetValue(step.Parameter, out JazzProgram sub))
                        throw new Exception($"Sub-flow '{step.Parameter}' not found.");

                    var subState = new ExecutionState();
                    RunSteps(sub.Steps, outcomes, consumed, subState);

                    if (!string.IsNullOrEmpty(subState.Bin))
                        state.Bin = subState.Bin;

                    string binport = subState.Binport;

                    if (!string.IsNullOrEmpty(binport))
                    {
                        string resolved = ResolveBinport(binport, step);

                        if (!string.IsNullOrEmpty(resolved))
                            state.Bin = resolved;

                        if (!string.IsNullOrEmpty(step.Result))
                        {
                            state.Result = step.Result;
                            return;
                        }
                    }
                    else if (subState.Result != null)
                    {
                        state.Result = subState.Result;
                        return;
                    }

                    index++;
                    continue;
                }

                if (cmd == "test")
                {
                    string name = step.Parameter;

                    int occurrence = consumed.TryGetValue(name, out int c) ? c : 0;
                    consumed[name] = occurrence + 1;

                    bool passed = true;
                    if (outcomes.TryGetValue(name, out List<bool> sequence) && sequence.Count > 0)
                    {
                        passed = occurrence < sequence.Count ? sequence[occurrence] : sequence[sequence.Count - 1];
                    }

                    bool branchTriggered;
                    string binValue;
                    string path;

                    if (passed)
                    {
                        // Result applies to a branch only when THAT
                        // branch's own Bin column is populated - not
                        // merely because a Path is set for it. A row
                        // can have PassPath (jump on pass) AND
                        // FailBin+Result (bin/finalize on fail) at the
                        // same time; without this the Result would
                        // wrongly leak onto a passing site.
                        branchTriggered = !string.IsNullOrEmpty(step.PassBin);
                        binValue = step.PassBin;
                        path = step.PassPath;
                    }
                    else
                    {
                        branchTriggered = !string.IsNullOrEmpty(step.FailBin);
                        binValue = step.FailBin;
                        path = step.FailPath;
                    }

                    if (!string.IsNullOrEmpty(binValue))
                    {
                        if (IsBinportToken(binValue))
                            state.Binport = binValue;
                        else
                            state.Bin = binValue;
                    }

                    if (branchTriggered && !string.IsNullOrEmpty(step.Result))
                    {
                        if (IsBinportToken(step.Result))
                            state.Binport = step.Result;
                        else
                            state.Result = step.Result;
                    }

                    if (state.Result != null && string.IsNullOrEmpty(path))
                        return;

                    if (!string.IsNullOrEmpty(path))
                    {
                        index = FindLabelIndex(steps, path);
                        continue;
                    }

                    index++;
                    continue;
                }

                if (cmd == "set-device")
                {
                    ApplyUnconditional(step, state);

                    if (state.Result != null)
                        return;

                    index++;
                    continue;
                }

                index++;
            }
        }

        private static void ApplyUnconditional(JazzStepDef step, ExecutionState state)
        {
            if (!string.IsNullOrEmpty(step.PassBin))
                state.Bin = step.PassBin;

            if (!string.IsNullOrEmpty(step.FailBin))
                state.Bin = step.FailBin;

            if (!string.IsNullOrEmpty(step.Result))
                state.Result = step.Result;
        }

        private static bool IsBinportToken(string value)
        {
            return Regex.IsMatch(value, @"^(fail|pass)\d+$", RegexOptions.IgnoreCase);
        }

        private static string ResolveBinport(string binport, JazzStepDef callStep)
        {
            Match match = Regex.Match(binport, @"^fail(\d+)$", RegexOptions.IgnoreCase);

            if (!match.Success)
                return null;

            int idx = int.Parse(match.Groups[1].Value);
            string list = callStep.FailBin;

            if (string.IsNullOrEmpty(list))
                return null;

            string[] parts = list.Split(',');

            if (idx < 0 || idx >= parts.Length)
                return null;

            string value = parts[idx].Trim();

            return string.IsNullOrEmpty(value) ? null : value;
        }

        private static int FindLabelIndex(List<JazzStepDef> steps, string label)
        {
            for (int i = 0; i < steps.Count; i++)
            {
                if (string.Equals(steps[i].Label, label, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            throw new Exception($"Label '{label}' not found.");
        }
    }
}