using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class NetworkBridge : NetworkBehaviour
{
    public static NetworkBridge Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SubmitMoveForwardServerRpc(string unitId) //unitOrigin jako surgat ID
    {
        ServiceMoveForwardClientRpc(unitId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ServiceMoveForwardClientRpc(string unitId)
    {
        Debug.Log("ServiceMoveForwardClientRpc dla jednostki " + unitId.ToString());

        UnitManager.Unit unit = UnitManager.Instance.UnitList.FirstOrDefault(u => u.ID.Equals(unitId));
        UnitManager.Instance.MoveForward(unit);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SubmitMoveLeftServerRpc(string unitId) //unitOrigin jako surgat ID
    {
        ServiceMoveLeftClientRpc(unitId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ServiceMoveLeftClientRpc(string unitId)
    {
        Debug.Log("ServiceMoveForwardClientRpc dla jednostki " + unitId.ToString());

        UnitManager.Unit unit = UnitManager.Instance.UnitList.FirstOrDefault(u => u.ID.Equals(unitId));
        UnitManager.Instance.MoveLeft(unit);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SubmitMoveRightServerRpc(string unitId) //unitOrigin jako surgat ID
    {
        ServiceMoveRightClientRpc(unitId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ServiceMoveRightClientRpc(string unitId)
    {
        Debug.Log("ServiceMoveForwardClientRpc dla jednostki " + unitId.ToString());

        UnitManager.Unit unit = UnitManager.Instance.UnitList.FirstOrDefault(u => u.ID.Equals(unitId));
        UnitManager.Instance.MoveRight(unit);
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SubmitBeMovedServerRpc(string unitId, int destinationQ, int destinationR) //unitOrigin jako surgat ID
    {
        ServiceBeMovedClientRpc(unitId, destinationQ, destinationR);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ServiceBeMovedClientRpc(string unitId, int destinationQ, int destinationR)
    {
        UnitManager.Unit unit = UnitManager.Instance.UnitList.FirstOrDefault(u => u.ID.Equals(unitId));
        UnitManager.Instance.BeMoved(unit, new HexCoords(destinationQ, destinationR));
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SubmitDestroyUnitServerRpc(string unitId) //unitOrigin jako surgat ID
    {
        ServiceDestroyUnitClientRpc(unitId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ServiceDestroyUnitClientRpc(string unitId)
    {
        UnitManager.Unit unit = UnitManager.Instance.UnitList.FirstOrDefault(u => u.ID.Equals(unitId));
        UnitManager.Instance.DestroyUnit(unit);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SubmitInitializeCombatRpc(string attakcerUnitId, string defenderUnitId)
    {
        ServiceInitializeCombatRpc(attakcerUnitId, defenderUnitId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ServiceInitializeCombatRpc(string attakcerUnitId, string defenderUnitId)
    {
        CombatManager.Instance.Initialize(attakcerUnitId, defenderUnitId);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SubmitCombatMoveRpc(HexDirectionChange direction, CombatRole combatRole)
    {
        ServiceCombatMoveRpc(direction, combatRole);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ServiceCombatMoveRpc(HexDirectionChange direction, CombatRole combatRole)
    {
        CombatManager.Instance.SetMove(direction, combatRole);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SubmitCombatDicesRpc(int d1, int? d2, int? d3, CombatRole combatRole)
    {
        ServiceCombatDicesRpc(d1, d2, d3, combatRole);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ServiceCombatDicesRpc(int d1, int? d2, int? d3, CombatRole combatRole)
    {
        CombatManager.Instance.SetDices(d1, d2, d3, combatRole);
    }


    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}
