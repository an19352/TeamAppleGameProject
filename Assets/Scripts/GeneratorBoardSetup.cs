using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorBoardSetup : MonoBehaviour
{
    public Transform generator, jumpPad, boxes;

    public void Setup(Vector3 generator_position , Vector3 jumpPad_position, Vector3 boxes_position, Quaternion boxes_rotation)
    {
        generator.position = transform.position + generator_position;
        jumpPad.position = transform.position + jumpPad_position;
        boxes.position = transform.position + boxes_position;
        boxes.rotation = boxes_rotation;
    }
}
