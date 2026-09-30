#if !CINEMACHINE_NO_CM2_SUPPORT
using System;
using UnityEngine;

using UnityEngine.Scripting.APIUpdating;
namespace Unity.Cinemachine
{
    /// <summary>
    /// Property applied to Vcam Target fields.  Used for custom drawing in the inspector.
    /// </summary>
    [Obsolete]
    [MovedFrom("Cinemachine")]
    public sealed class VcamTargetPropertyAttribute : PropertyAttribute { }

}
#endif
