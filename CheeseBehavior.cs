/********************************************************
* CheeseBehavior.cs
* Austen Mesaros
*
* This class rotates the cheese
*********************************************************/
using UnityEngine;

public class CheeseBehavior : MonoBehaviour {

    private const float rotationSpeed = .5f;

    // update is called once per frame
    void Update() => this.transform.Rotate(new Vector3(0, rotationSpeed, 0));
}
