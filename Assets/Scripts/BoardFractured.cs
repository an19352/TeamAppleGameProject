using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardFractured : MonoBehaviour
{
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Transform[] pieces = GetComponentsInChildren<Transform>();
            var piecesList = new List<Transform>(pieces);
            var nonRigidPieces = piecesList.FindAll(piece => !piece.GetComponent<Rigidbody>());

            int r = Random.Range(20, 20);

            if (nonRigidPieces.Count < r)
            {
                r = nonRigidPieces.Count;
            };
            for (int i = 0; i < r; i++)
            {
                var rb = nonRigidPieces[i].gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.useGravity = false;
            }

        };
    }

}
