using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;

namespace Console.PowerPack.Core
{
    internal static class ExtensionMethods
    {
        private const int VariantSize = 16;

        public static SnapshotPoint GetStart(this ITextSnapshot snapshot)
        {
            return new SnapshotPoint(snapshot, 0);
        }

        public static SnapshotPoint GetEnd(this ITextSnapshot snapshot)
        {
            return new SnapshotPoint(snapshot, snapshot.Length);
        }

        public static NormalizedSnapshotSpanCollection TranslateTo(this NormalizedSnapshotSpanCollection coll,
            ITextSnapshot snapshot, SpanTrackingMode spanTrackingMode)
        {
            if (coll.Count > 0 && coll[0].Snapshot != snapshot)
                return
                    new NormalizedSnapshotSpanCollection(
                        coll.Select(span => span.TranslateTo(snapshot, spanTrackingMode)));
            return coll;
        }

        public static void ClearReadOnlyRegion(this IReadOnlyRegionEdit readOnlyRegionEdit,
            ref IReadOnlyRegion readOnlyRegion)
        {
            if (readOnlyRegion == null)
                return;
            readOnlyRegionEdit.RemoveReadOnlyRegion(readOnlyRegion);
            readOnlyRegion = null;
        }

        public static void Raise<T>(this EventHandler<EventArgs<T>> ev, object sender, T arg)
        {
            if (ev == null)
                return;
            ev(sender, new EventArgs<T>(arg));
        }

        public static void Execute(this IOleCommandTarget target, Guid guidCommand, uint idCommand, object args = null)
        {
            IntPtr num = IntPtr.Zero;
            try
            {
                if (args != null)
                {
                    num = Marshal.AllocHGlobal(16);
                    Marshal.GetNativeVariantForObject(args, num);
                }
                ErrorHandler.ThrowOnFailure(target.Exec(ref guidCommand, idCommand, 0U, num, IntPtr.Zero));
            }
            finally
            {
                if (num != IntPtr.Zero)
                {
                    VariantClear(num);
                    Marshal.FreeHGlobal(num);
                }
            }
        }

        public static void Execute(this IOleCommandTarget target, VSConstants.VSStd2KCmdID idCommand, object args = null)
        {
            target.Execute(VSConstants.VSStd2K, (uint) idCommand, args);
        }

        [DllImport("Oleaut32.dll", PreserveSig = false)]
        private static extern void VariantClear(IntPtr var);
    }
}