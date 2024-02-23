using System;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A simple sample showing how to use the Relay Allocation package. As the host, you can authenticate, request a relay allocation, get a join code and join the allocation.
/// </summary>
/// <remarks>
/// The sample is limited to calling the Relay Allocation Service and does not cover connecting the host game client to the relay using Unity Transport Protocol.
/// This will cause the allocation to be reclaimed about 10 seconds after creating it.
/// </remarks>
public class SimpleRelay : MonoBehaviour
{
    /// <summary>
    /// The textbox displaying the Player Id.
    /// </summary>
    public Text PlayerIdText;

    /// <summary>
    /// The dropdown displaying the region.
    /// </summary>
    [Header("Region(区域)下拉框")]
    public Dropdown RegionsDropdown;

    /// <summary>
    /// The textbox displaying the Allocation Id.
    /// </summary>
    public Text HostAllocationIdText;

    /// <summary>
    /// The textbox displaying the Join Code.
    /// </summary>
    public Text JoinCodeText;

    /// <summary>
    /// The textbox displaying the Allocation Id of the joined allocation.
    /// </summary>
    public Text PlayerAllocationIdText;

    Guid hostAllocationId;
    Guid playerAllocationId;
    string allocationRegion = "";
    string joinCode = "n/a";
    string playerId = "Not signed in";
    string autoSelectRegionName = "auto-select (QoS)";
    int regionAutoSelectIndex = 0;
    List<Region> regions = new List<Region>();
    List<string> regionOptions = new List<string>();


    async void Start()
    {
        await UnityServices.InitializeAsync();

        UpdateUI();
    }

    void UpdateUI()
    {
        PlayerIdText.text = playerId;
        RegionsDropdown.interactable = regions.Count > 0;
        RegionsDropdown.options?.Clear();
        RegionsDropdown.AddOptions(new List<string> {autoSelectRegionName});  // index 0 is always auto-select (use QoS)
        RegionsDropdown.AddOptions(regionOptions);
        if (!String.IsNullOrEmpty(allocationRegion))
        {
            if (regionOptions.Count == 0)
            {
                RegionsDropdown.AddOptions(new List<String>(new[] { allocationRegion }));
            }
            RegionsDropdown.value = RegionsDropdown.options.FindIndex(option => option.text == allocationRegion);
        }
        HostAllocationIdText.text = hostAllocationId.ToString();
        JoinCodeText.text = joinCode;
        PlayerAllocationIdText.text = playerAllocationId.ToString();
    }

    #region 界面的按钮事件，挂载
    /// <summary>
    /// 登录Auth包.
    /// </summary>
    public async void OnSignIn() {
        // auth包 匿名登录
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        // 获得playerID
        playerId = AuthenticationService.Instance.PlayerId;

        Debug.Log($"Signed in. Player ID: {playerId}");
        UpdateUI();
    }

    /// <summary>
    /// 切换服务器所在地区.
    /// </summary>
    public async void OnRegion() {
        Debug.Log("Host - Getting regions.");
        // ListRegionsAsync ： Lists all available regions with relay servers
        var allRegions = await RelayService.Instance.ListRegionsAsync();
        regions.Clear();
        regionOptions.Clear();
        foreach (var region in allRegions) {
            Debug.Log(region.Id + ": " + region.Description);
            regionOptions.Add(region.Id);
            regions.Add(region);
        }
        UpdateUI();
    }

    /// <summary>
    /// Event handler for when the Allocate(分配) button is clicked.
    /// </summary>
    public async void OnAllocate() {
        Debug.Log("Host - Creating an allocation.");

        // Determine region to use (user-selected or auto-select/QoS)
        string region = GetRegionOrQosDefault();
        Debug.Log($"The chosen region is: {region ?? autoSelectRegionName}");

        // Important: Once the allocation is created, you have ten seconds to BIND
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4, region);
        hostAllocationId = allocation.AllocationId;
        allocationRegion = allocation.Region;

        Debug.Log($"Host Allocation ID: {hostAllocationId}, region: {allocationRegion}");

        UpdateUI();
    }

    string GetRegionOrQosDefault() {
        // Return null (indicating to auto-select the region/QoS) if regions list is empty OR auto-select/QoS is chosen
        if (!regions.Any() || RegionsDropdown.value == regionAutoSelectIndex) {
            return null;
        }
        // else use chosen region (offset -1 in dropdown due to first option being auto-select/QoS)
        return regions[RegionsDropdown.value - 1].Id;
    }

    /// <summary>
    /// Event handler for when the Get Join Code button is clicked.
    /// </summary>
    public async void OnJoinCode() {
        Debug.Log("Host - Getting a join code for my allocation. I would share that join code with the other players so they can join my session.");

        try {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocationId);
            Debug.Log("Host - Got join code: " + joinCode);
        } catch (RelayServiceException ex) {
            Debug.LogError(ex.Message + "\n" + ex.StackTrace);
        }

        UpdateUI();
    }

    /// <summary>
    /// Event handler for when the Join button is clicked.
    /// </summary>
    public async void OnJoin() {
        Debug.Log("Player - Joining host allocation using join code.");

        try {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            playerAllocationId = joinAllocation.AllocationId;
            Debug.Log("Player Allocation ID: " + playerAllocationId);
        } catch (RelayServiceException ex) {
            Debug.LogError(ex.Message + "\n" + ex.StackTrace);
        }

        UpdateUI();
    } 
    #endregion
}
