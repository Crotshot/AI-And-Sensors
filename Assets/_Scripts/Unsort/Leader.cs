using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leader : MonoBehaviour {
    
    [SerializeField]List<Orange_AI> followers;//Max five followers

    static float XMOD = 0.5f, ZMOD = 0.5f;
    Vector3[] followerPositions;
    virtual protected void Awake() {
        followers = new List<Orange_AI>();
        followerPositions = new Vector3[5];
        followerPositions[0] = new Vector3(1.5f * XMOD, 0, -6 * ZMOD);
        followerPositions[1] = new Vector3(-1.5f * XMOD,0, -6 * ZMOD);
        followerPositions[2] = new Vector3(-4.5f * XMOD,0, -6 * ZMOD);
        followerPositions[3] = new Vector3(4.5f * XMOD, 0, -6 * ZMOD);
        followerPositions[4] = new Vector3(0,           0, -7.5f * ZMOD);
    }

    virtual public bool AddFollower(Orange_AI ai) {
        CheckFollowers();
        if (followers.Count < 5) {
            followers.Add(ai);
            Debug.Log("Follower Added");
            return true;
        }
        else {
            Debug.Log("Follower Refused");
            return false;
        }
    }

    virtual protected void Update() {
        int count = 0;
        CheckFollowers();
        foreach (Orange_AI follower in followers) {
            follower.isFollowing(transform.TransformPoint(followerPositions[count]));
            count++;
        }
    }

    virtual protected void CheckFollowers() {
        foreach (Orange_AI i in followers) {
            if (i == null) {
                followers.Remove(i);
            }
        }
    }

    virtual protected void Attack(Transform target) {
        CheckFollowers();
        foreach (Orange_AI o in followers) {
            if (o.CommandedForward(target)) {
                Debug.Log("Attack Success");
                return;
            }
        }
        Debug.Log("Attack Fail");
    }

    virtual protected void Recall() {
        CheckFollowers();
        foreach (Orange_AI o in followers) {
            if (o.CommandedRecall()) {
                Debug.Log("Recall Success");
                return;
            }
        }
        Debug.Log("Recall Fail");
    }

    public void Death() {
        foreach (Orange_AI o in followers) {
            o.WipeLeader();
        }
        Destroy(this);
    }

    virtual public void FollowerDeath(Orange_AI ai) {
        followers.Remove(ai);
        GetComponent<Purple_AI>().Flee();
    }
}