﻿using System.Collections.Generic;
using System.Linq;
using Gallio.Model.Filters;
using Gallio.Runner;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace TestPlatform.Gallio
{
    public class TestRunner
    {
        private readonly ITestCaseFactory testCaseFactory;
        private readonly ITestResultFactory testResultFactory;
        private readonly TestProperty testIdProperty;
        private readonly TestProperty filePathProperty;
        private TestLauncher launcher;

        public TestRunner(ITestCaseFactory testCaseFactory, ITestResultFactory testResultFactory, 
            TestProperty testIdProperty, TestProperty filePathProperty)
        {
            this.testCaseFactory = testCaseFactory;
            this.testResultFactory = testResultFactory;
            this.testIdProperty = testIdProperty;
            this.filePathProperty = filePathProperty;
        }

        public void Cancel()
        {
            launcher.Cancel();
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, ITestExecutionRecorder testExecutionRecorder)
        {
            launcher = new TestLauncher();
            
            foreach (var source in sources)
            {
                launcher.AddFilePattern(source);
            }

            RunTests(runContext, testExecutionRecorder, sources);
        }

        private void RunTests(IRunContext runContext, ITestExecutionRecorder testExecutionRecorder, IEnumerable<string> source)
        {
            if (runContext.InIsolation)
                launcher.TestProject.TestRunnerFactoryName = StandardTestRunnerFactoryNames.IsolatedAppDomain;

            var extension = new VSTestWindowExtension(testExecutionRecorder, testCaseFactory, testResultFactory, source);

            launcher.TestProject.AddTestRunnerExtension(extension);
            launcher.Run();
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, ITestExecutionRecorder testExecutionRecorder)
        {
            launcher = new TestLauncher();

            var source = tests.GroupBy(tc => tc.Source).Select(testCaseGroup => testCaseGroup.Key).ToList();

            foreach (var test in tests)
            {
                var filePath = test.GetPropertyValue(filePathProperty, "");
                launcher.AddFilePattern(filePath);
            }

            SetTestFilter(tests);

            RunTests(runContext, testExecutionRecorder, source);
        }

        private void SetTestFilter(IEnumerable<TestCase> tests)
        {
            var filters = tests.Select(t => new EqualityFilter<string>(t.GetPropertyValue(testIdProperty).ToString())).ToArray();
            var filterSet = new FilterSet<ITestDescriptor>(new IdFilter<ITestDescriptor>(new OrFilter<string>(filters)));
            launcher.TestExecutionOptions.FilterSet = filterSet;
        } 
    }
}