#if !CINEMACHINE_NO_CM2_SUPPORT
using System;
using UnityEngine;

using UnityEngine.Scripting.APIUpdating;
namespace Unity.Cinemachine
{
    /// <summary>
    /// This is a deprecated component.
    /// </summary>
    [Obsolete("CinemachinePipeline has been deprecated.")]
    [AddComponentMenu("")] // Don't display in add component menu
    [MovedFrom("Cinemachine")]
    public sealed class CinemachinePipeline : MonoBehaviour
    {
    }
}
#endif
