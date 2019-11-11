﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerHealth : NetworkBehaviour, Damagable {
    private PlayerController playerController;
    private ScoreDisplayer scoreDisplayer;
    private TeamManager teamManager;
    void Start() {
        playerController = GetComponent<PlayerController>();
        teamManager = GetComponent<TeamManager>();
    }

    public override void OnStartServer()
    {
        scoreDisplayer = FindObjectOfType<ScoreDisplayer>();
    }

    public void DealDamage(GameObject from, int damage) {
        CmdDealDamage(from, damage);
    }

    [Command]
    public void CmdDealDamage(GameObject from, int damage)
    {
        if(from != null)
        {
            TeamManager fromTeamManger = from.GetComponent<TeamManager>();
            if(fromTeamManger != null)
            {
                if(scoreDisplayer != null)
                {
                    if(fromTeamManger.GetTeam() != teamManager.GetTeam())
                    {
                        scoreDisplayer.RpcAddKill(fromTeamManger.GetTeam());
                    }
                }
            }
        }
        playerController.RpcDie();
    }
}
