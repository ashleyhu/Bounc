using UnityEngine;
using System.Collections;

public class Rotater : MonoBehaviour {

	void Start() {
		this.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-5, 5), Random.Range(-5, 5));
//		this.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-5, 0), 0f);
	}
	// Update is called once per frame
	void Update () {
		transform.Rotate(new Vector3(0, 0, 45) * Time.deltaTime);
	}
}
