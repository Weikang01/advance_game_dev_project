using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

enum OptType {
    Move     = 1,
    Attack1  = 2,
    Attack2  = 3,
    Attack3  = 4,
    Skill1   = 5,
    Skill2   = 6,
    Skill3   = 7,
    Jump     = 8,
    Dead     = 9,
}

public class GameZygote : MonoBehaviour
{
    // local test
    //public Joystick stick;  // Reference to the joystick
    // end
    public GameObject[] character_prefab = null;  // Array of characters
    public GameObject[] spawn_points = null;  // Array of spawn points
    // Start is called before the first frame update

    private int SyncFrameID = 1;  // Synchronized frame ID
    private FrameOpts lastFrameOpt = null;
    
    public const int LOGIC_DELTA_TIME = 66;  // 66 ms
    private const int TEAM_SIZE = 2;

    internal Dictionary<int, Dictionary<int, Player>> ingame_characters = new Dictionary<int, Dictionary<int, Player>>(); // teamid -> seatid : character list
    internal Dictionary<int, Rope> ropes = new Dictionary<int, Rope>();  // ropeid -> rope
    private int current_rope_index = 0;
    public Vector3 camera_offset = Vector3.zero;

    [Header("Character Setting")]
    public float character_speed = 2.0f;  // Character speed

    [Header("Game Finish Window")]
    public GameFinish GameFinish;

    void CapturePlayerOpts()
    {
        NextFrameOpt nextFrameOpt = new NextFrameOpt();
        nextFrameOpt.Frameid = SyncFrameID + 1;
        nextFrameOpt.Zid = UGame.Instance.zoneid;
        nextFrameOpt.Matchid = UGame.Instance.matchid;
        nextFrameOpt.Seatid = UGame.Instance.self_seatid;

        // joystick
        OptionEvent opt_move = new OptionEvent();
        opt_move.Seatid = UGame.Instance.self_seatid;
        if (ingame_characters[UGame.Instance.self_teamid][UGame.Instance.self_seatid] != null &&
            ingame_characters[UGame.Instance.self_teamid][UGame.Instance.self_seatid].characterController.Grounded && EventListener.Instance.IsSpaceDown())
        {
            opt_move.OptType = (int)OptType.Jump;
        }
        else if (ingame_characters[UGame.Instance.self_teamid][UGame.Instance.self_seatid] != null &&
            ingame_characters[UGame.Instance.self_teamid][UGame.Instance.self_seatid].characterController.OnLadder)
        {
            opt_move.OptType = (int)OptType.Move;
            opt_move.Y = BitConverter.ToInt32(BitConverter.GetBytes(character_speed * Input.GetAxisRaw("Vertical")));
        }
        else
        {
            opt_move.OptType = (int)OptType.Move;
        }
        opt_move.X = BitConverter.ToInt32(BitConverter.GetBytes(character_speed * Input.GetAxisRaw("Horizontal")));
        //Debug.Log("X: " + opt_move.X + " Y: " + opt_move.Y + " type: " + opt_move.GetType());
        //Debug.Log("Send: X: " + opt_move.X + " Y: " + opt_move.Y + " OptType: " + opt_move.OptType);
        nextFrameOpt.Opts.Add(opt_move);

        if (ingame_characters[UGame.Instance.self_teamid][UGame.Instance.self_seatid].is_dead)
        {
            OptionEvent opt_die = new OptionEvent();
            opt_die.Seatid = UGame.Instance.self_seatid;
            opt_die.OptType = (int)OptType.Dead;
            nextFrameOpt.Opts.Add(opt_die);
        }
        // attack

        // send to server
        LogicServiceProxy.Instance.SendNextFrameOpts(nextFrameOpt);

        SyncFrameID = nextFrameOpt.Frameid;
    }

    void SyncLastLogicFrame(FrameOpts frameOpts)
    {
        for (int i = 0; i < frameOpts.Opts.Count; i++)
        {
            int seatid = frameOpts.Opts[i].Seatid;
            Player character = GetCharacter(seatid);
            if (character == null)
            {
                Debug.LogError("Character not found: " + seatid);
                continue;
            }

            //character.SyncLastLogicFrame(frameOpts.Opts[i]);
            if (frameOpts.Opts[i].OptType == (int)OptType.Dead)
            {
                foreach (KeyValuePair<int, Player> entry in ingame_characters[character.team_id])
                {
                    entry.Value.SyncLastLogicFrame(frameOpts.Opts[i]);
                }
            } else
            {
                character.SyncLastLogicFrame(frameOpts.Opts[i]);
            }
        }
        // creeps AI
    }

    private void JumpToNextFrame(FrameOpts frameOpts)
    {
        for (int i = 0; i < frameOpts.Opts.Count; i++)
        {
            int seatid = frameOpts.Opts[i].Seatid;
            Player character = GetCharacter(seatid);
            if (character == null)
            {
                Debug.LogError("Character not found: " + seatid);
                continue;
            }
            //character.JumpToNextFrame(frameOpts.Opts[i]);

            if (frameOpts.Opts[i].OptType == (int)OptType.Dead)
            {
                foreach (KeyValuePair<int, Player> entry in ingame_characters[character.team_id])
                {
                    entry.Value.JumpToNextFrame(frameOpts.Opts[i]);
                }
            }
            else
            {
                character.JumpToNextFrame(frameOpts.Opts[i]);
            }
        }

        // creeps AI
    }

    private void OnLogicUpdate(string name, object udata)
    {
        LogicFrame logicFrame = (LogicFrame)udata;
        if (logicFrame.Frameid < this.SyncFrameID)
            return;

        // synchronize last logic operation, adjust position, etc
        if (this.lastFrameOpt != null)
        {
            this.SyncLastLogicFrame(this.lastFrameOpt);
        }

        // start sync frame from SyncFrameID + 1 to logicFrame.frameid - 1;  // sync missing frames
        for (int i = 0; i < logicFrame.UnsyncFrames.Count; i++)
        {
            if (this.SyncFrameID >= logicFrame.UnsyncFrames[i].Frameid)
                continue;
            else if (logicFrame.UnsyncFrames[i].Frameid >= logicFrame.Frameid)
                break;

            JumpToNextFrame(logicFrame.UnsyncFrames[i]);
        }

        // get latest operation, play animation based on operation, etc
        this.SyncFrameID = logicFrame.Frameid;
        if (logicFrame.UnsyncFrames.Count > 0)
        {
            this.lastFrameOpt = logicFrame.UnsyncFrames[logicFrame.UnsyncFrames.Count - 1];
            this.HandleFrameEvent(this.lastFrameOpt);
        }
        else
        {
            this.lastFrameOpt = null;
        }

        // gather operation of next frame, send to server
        CapturePlayerOpts();
    }

    private void HandleFrameEvent(FrameOpts frameOpts)
    {
        // execute all operations from every players
        for (int i = 0; i < frameOpts.Opts.Count; i++)
        {

            int seatid = frameOpts.Opts[i].Seatid;
            Player character = GetCharacter(seatid);
            if (character == null)
            {
                Debug.LogError("Character not found: " + seatid);
                continue;
            }

            //character.HandleFrameEvent(frameOpts.Opts[i]);
            if (frameOpts.Opts[i].OptType == (int)OptType.Dead)
            {
                foreach (KeyValuePair<int, Player> entry in ingame_characters[character.team_id])
                {
                    entry.Value.HandleFrameEvent(frameOpts.Opts[i]);
                }
            }
            else
            {
                character.HandleFrameEvent(frameOpts.Opts[i]);
            }
        }

        // creeps AI
    }

    GameObject InstantiateCharacterObject(int character_avatar_id)
    {
        GameObject character = GameObject.Instantiate(this.character_prefab[(character_avatar_id - 1) % this.character_prefab.Length]);

        return character;
    }

    private void OnGameFinished(string name, object udata)
    {
        GameFinishedRes res = (GameFinishedRes)udata;

        GameFinish.gameObject.SetActive(true);
        GameFinish.OnGameFinished(res.WinnerTeamid);
    }

    private void OnPlayerRespawned(string name, object udata)
    {
        int rope_index = (int)udata;
        InitiateRope(
            ropes[rope_index].endA.GetComponent<Player>(),
            ropes[rope_index].endB.GetComponent<Player>());
    }

    void OnEnable()
    {
        EventManager.Instance.AddEventListener("onLogicUpdate", OnLogicUpdate);
        EventManager.Instance.AddEventListener("gameFinished", OnGameFinished);
        EventManager.Instance.AddEventListener("playerRespawned", OnPlayerRespawned);

        for (int i = 0; i < UGame.Instance.match_players_info.Count; i++)
        {
            this.PlaceCharacterAt(UGame.Instance.match_players_info[i]);
        }
    }

    private Vector3 GetSpawnPointAroundOrigin(Vector3 origin, float offset, int index)
    {
        //Debug.Log("origin: " + origin + "\toffset: " + offset + "\tindex: " + index);
        float angle = (index * 2 * Mathf.PI) / TEAM_SIZE;

        return new Vector3(
            origin.x + offset * Mathf.Cos(angle),
            origin.y,
            origin.z + offset * Mathf.Sin(angle)
            );
    }

    Player GetCharacter(int seatid)
    {
        foreach (KeyValuePair<int, Dictionary<int, Player>> item in ingame_characters)
        {
            if (item.Value.ContainsKey(seatid))
            {
                return item.Value[seatid];
            }
        }

        return null;
    }

    void InitiateRope(Player player_A, Player player_B)
    {
        GameObject rope_obj = new GameObject();
        rope_obj.layer = LayerMask.NameToLayer("OnlyInteractWithGround");
        rope_obj.transform.SetParent(this.GetComponentInParent<Transform>());
        Rope rope = rope_obj.AddComponent<Rope>();
        rope_obj.transform.position = (player_A.transform.position + player_B.transform.position) / 2;

        rope_obj.name = "rope_" + player_A.name + "_" + player_B.name;

        rope.endA = player_A.gameObject;
        rope.endB = player_B.gameObject;

        if (player_A.rope_index == -1 && player_B.rope_index == -1)
        {
            player_A.rope_index = current_rope_index;
            player_B.rope_index = current_rope_index;

            ropes.Add(current_rope_index, rope);

            current_rope_index++;
        }
        else
        {
            if (ropes[player_A.rope_index])
            {
                Destroy(ropes[player_A.rope_index].gameObject);
            }

            player_B.rope_index = player_A.rope_index;
            ropes[player_A.rope_index] = rope;
        }
    }

    Player PlaceCharacterAt(CharacterInfo characterInfo)
    {
        UserInfo uinfo = UGame.Instance.GetUserInfo(characterInfo.Seatid);

        GameObject c_object = InstantiateCharacterObject(uinfo.uSystemAvatar);
        c_object.name = uinfo.uNick;
        c_object.transform.SetParent(this.transform, false);

        if (!ingame_characters.ContainsKey(characterInfo.Teamid))
        {
            ingame_characters.Add(characterInfo.Teamid, new Dictionary<int, Player>());
        }
        c_object.transform.position = GetSpawnPointAroundOrigin(
            this.spawn_points[(characterInfo.Teamid - 1) % this.spawn_points.Length].transform.position, 3.0f, ingame_characters[characterInfo.Teamid].Count);
        Player control = c_object.AddComponent<Player>();
        control.spawn_point = c_object.transform.position;
        control.is_ghost = characterInfo.Seatid != UGame.Instance.self_seatid;
        control.LogicInit(c_object.transform.position);
        control.team_id = characterInfo.Teamid;
        control.seat_id = characterInfo.Seatid;
        control.camera_offset = this.camera_offset;
        //Debug.Log("ingame_characters[characterInfo.Teamid].Count: " + ingame_characters[characterInfo.Teamid].Count);
        if (ingame_characters[characterInfo.Teamid].Count % 2 == 1)
        {
            Player previous_character = ingame_characters[characterInfo.Teamid].Last().Value;

            InitiateRope(previous_character, control);

            ingame_characters[characterInfo.Teamid].Add(characterInfo.Seatid, control);
            //rope.Initialize();
        }
        else
        {
            ingame_characters[characterInfo.Teamid].Add(characterInfo.Seatid, control);
        }

        return control;
    }

    void OnDisable()
    {
        //EventManager.Instance.RemoveEventListener("onLogicUpdate", this.OnLogicUpdate);
        EventManager.Instance.RemoveEventListener("onLogicUpdate", OnLogicUpdate);
        EventManager.Instance.RemoveEventListener("gameFinished", OnGameFinished);
        EventManager.Instance.RemoveEventListener("playerRespawned", OnPlayerRespawned);
        UGame.Instance.QuitMatch();
    }
}
