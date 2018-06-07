//
// TestPriorityOrderer.cs
//
// Author:
//       MiNG <developer@ming.gz.cn>
//
// Copyright (c) 2018 Alibaba Cloud
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using Aliyun.Api.LogService.Utils;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Aliyun.Api.LogService.Tests.TestUtils
{
    public class TestPriorityOrderer : ITestCaseOrderer
    {
        private static readonly String[] EmptyDepends = { };

        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
        {
            var cases = testCases.ToArray();

            var caseDepends = cases.ToDictionary(
                x => x.TestMethod.Method.Name,
                x =>
                    (
                        depends: x.TestMethod.Method
                                     .GetCustomAttributes(typeof(TestPriorityAttribute).AssemblyQualifiedName)
                                     .SingleOrDefault()?
                                     .GetNamedArgument<String[]>("DependsOn") ?? EmptyDepends,
                        // Use Int64 to prevent the after-last case.
                        priority: (Int64) (x.TestMethod.Method
                                               .GetCustomAttributes(typeof(TestPriorityAttribute).AssemblyQualifiedName)
                                               .SingleOrDefault()?
                                               .GetNamedArgument<Int32>("Priority") ?? 0)
                    ));

            var allCases = caseDepends
                .ToDictionary(x => x.Key, x => new DependNode(x.Key));

            // Construct hierarchy
            foreach (var (current, (depends, _)) in caseDepends)
            {
                foreach (var depend in depends)
                {
                    if (allCases.TryGetValue(depend, out var @case))
                    {
                        @case.TryAdd(allCases[current]);
                    }
                }
            }

            var prioritizedCases = allCases.ToDictionary(
                x => x.Key,
                x => caseDepends[x.Key].depends.IsEmpty()
                    ? caseDepends[x.Key].priority // Root nodes use defined priority. 
                    : (Int64?)null);              // Child nodes use hierarchy related priority.

            // Process all roots.
            var rootNodes = allCases
                // No depends
                .Where(x => caseDepends[x.Key].depends.IsEmpty())
                // Contain children
                .Where(x => allCases[x.Key].Children.IsNotEmpty())
                .ToArray(); // Freeze for debugging.
            foreach (var (rootName, rootNode) in rootNodes)
            {
                // Circular validation context.
                var path = new Stack<String>();

                void DfsProcessHierarchy(IEnumerable<KeyValuePair<String, DependNode>> nodes, Int64 priority)
                {
                    foreach (var (currentName, currentNode) in nodes)
                    {
                        // Check cirular reference.
                        if (path.Contains(currentName))
                        {
                            throw new ArgumentException($"Circular reference detect: [{String.Join("->", path.Append(currentName))}]");
                        }

                        path.Push(currentName);
                        prioritizedCases[currentName] = Math.Max(prioritizedCases[currentName] ?? priority, priority);

                        // Deep-First traverse
                        DfsProcessHierarchy(currentNode.Children, priority + 1);
                        path.Pop();
                    }
                }

                path.Push(rootName);
                var rootPriority = prioritizedCases[rootName] ?? 0;
                DfsProcessHierarchy(rootNode.Children, rootPriority + 1);
                path.Pop();
            }

            var orderedCases = cases
                .OrderBy(x => prioritizedCases[x.TestMethod.Method.Name] ?? 0)
                .ToArray();

            return orderedCases;
        }

        private class DependNode
        {
            public String Name { get; }

            public IDictionary<String, DependNode> Children { get; } = new Dictionary<String, DependNode>();

            public DependNode(String name)
            {
                this.Name = name;
            }

            public Boolean TryAdd(DependNode child)
            {
                return this.Children.TryAdd(child.Name, child);
            }
        }
    }
}
