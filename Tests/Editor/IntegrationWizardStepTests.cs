using System.Reflection;
using NUnit.Framework;
using LiveGameLink.Editor;

namespace LiveGameLink.Tests.Editor
{
    /// Wizard step state machine: each step reachable iff every prior step completed.
    public class IntegrationWizardStepTests
    {
        IntegrationWizard _w;
        FieldInfo _completed;
        MethodInfo _isReachable;
        System.Type _stepEnum;

        [SetUp]
        public void Setup()
        {
            _w = UnityEngine.ScriptableObject.CreateInstance<IntegrationWizard>();
            var t = typeof(IntegrationWizard);
            _completed   = t.GetField("_completed", BindingFlags.NonPublic | BindingFlags.Instance);
            _isReachable = t.GetMethod("IsReachable", BindingFlags.NonPublic | BindingFlags.Instance);
            _stepEnum    = t.GetNestedType("Step", BindingFlags.NonPublic);
            Assert.IsNotNull(_completed);
            Assert.IsNotNull(_isReachable);
            Assert.IsNotNull(_stepEnum);
        }

        [TearDown] public void Teardown() { UnityEngine.Object.DestroyImmediate(_w); }

        bool Reachable(int idx) => (bool)_isReachable.Invoke(_w, new[] { System.Enum.ToObject(_stepEnum, idx) });
        void SetCompleted(params int[] idxs)
        {
            var arr = (bool[])_completed.GetValue(_w);
            for (int i = 0; i < arr.Length; i++) arr[i] = false;
            foreach (var i in idxs) arr[i] = true;
        }

        [Test] public void Step0AlwaysReachable() { SetCompleted(); Assert.IsTrue(Reachable(0)); }

        [Test] public void Step1RequiresStep0()
        {
            SetCompleted();
            Assert.IsFalse(Reachable(1));
            SetCompleted(0);
            Assert.IsTrue(Reachable(1));
        }

        [Test] public void LastStepRequiresAllPrior()
        {
            int last = System.Enum.GetValues(_stepEnum).Length - 1;
            SetCompleted();
            Assert.IsFalse(Reachable(last));
            var all = new int[last];
            for (int i = 0; i < last; i++) all[i] = i;
            SetCompleted(all);
            Assert.IsTrue(Reachable(last));
        }

        [Test] public void SkippingABlocksProgress()
        {
            SetCompleted(0, 1, 3); // skip 2
            Assert.IsFalse(Reachable(4));
        }
    }
}
