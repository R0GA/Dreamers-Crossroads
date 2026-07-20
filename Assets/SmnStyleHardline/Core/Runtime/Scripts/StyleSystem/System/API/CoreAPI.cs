namespace SmnStyleHardline.Core
{
    /*
 * Core API
 * Exposes access to global system state and core runtime controls
 * for the SSH style system.
 */

using UnityEngine;


// ==========================================================================
// Core API
// ==========================================================================

public sealed class CoreAPI
{
    readonly SSHStyleManager manager;

    internal CoreAPI(SSHStyleManager manager)
    {
        this.manager = manager;
    }


    // ======================================================================
    // System Asset
    // ======================================================================

    /// <summary>
    /// Returns the currently active system asset.
    /// </summary>
    /// <returns>
    /// The active <see cref="SSHSystemAsset"/>, or null if none is assigned.
    /// </returns>
    public SSHSystemAsset GetSystemAsset()
    {
        return manager.GetSystemAsset_Internal();
    }

    /// <summary>
    /// Assigns a new system asset and reinitializes all style controllers.
    /// </summary>
    /// <param name="asset">The system asset to assign.</param>
    public void SetSystemAsset(SSHSystemAsset asset)
    {
        manager.SetSystemAsset_Internal(asset);
    }

    /// <summary>
    /// Returns whether a system asset is currently assigned.
    /// </summary>
    /// <returns>
    /// True if a system asset is present; otherwise false.
    /// </returns>
    public bool HasSystemAsset()
    {
        return manager.GetSystemAsset_Internal() != null;
    }


    // ======================================================================
    // Core Controls
    // ======================================================================

    /// <summary>
    /// Sets the frame interval used for environment light synchronization.
    /// </summary>
    /// <param name="tick">
    /// Number of frames between environment light updates.
    /// Values less than one are clamped internally.
    /// </param>
    public void SetLightSyncTick(int tick)
    {
        manager.SetLightSyncTick_Internal(tick);
    }

    /// <summary>
    /// Returns the current environment light synchronization interval.
    /// </summary>
    /// <returns>
    /// The number of frames between environment light updates.
    /// </returns>
    public int GetLightSyncTick()
    {
        return manager.GetLightSyncTick_Internal();
    }

    /// <summary>
    /// Forces an immediate environment logic update.
    /// </summary>
    public void ForceEnvironmentTick()
    {
        manager.ForceEnvironmentTick_Internal();
    }
}

}