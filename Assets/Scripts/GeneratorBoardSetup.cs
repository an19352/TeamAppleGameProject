using System.Collections;
using System.Collections.Generic;
using System.Security;
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

    public void Setup()
    { 
        Vector3 gen,box;

        gen = generator.position;
        gen.x = transform.position.x - (generator.position.x - transform.position.x);
        generator.position = gen;
        generator.rotation = Quaternion.identity;
        
        box = boxes.position;
        box.x = transform.position.x - (boxes.position.x - transform.position.x);
        boxes.position = box;
        boxes.rotation = Quaternion.identity;
    }
}
