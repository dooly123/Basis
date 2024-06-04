using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoleBehavior : MonoBehaviour
{
    [SerializeField] float Height, Origin;
    void Start()
    {
        StartCoroutine(OutOfHole());
    }

    void Update()
    {
        Vector3 _Target = Camera.main.transform.position;
        _Target.y = transform.position.y;
        transform.LookAt(_Target);
    }

    private void OnTriggerEnter(Collider other)
    {
        Hit = true;
    }

    bool Hit = false;
    IEnumerator OutOfHole()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1f, 10f));
            while (!Hit)
            {
                if (transform.localPosition.y <= Height)
                {
                    transform.localPosition += Vector3.up * 0.001f;
                }
                yield return new WaitForFixedUpdate();
            }
            if (Hit)
            {
                while (transform.localPosition.y > Origin)
                {
                    transform.localPosition -= Vector3.up * 0.001f;
                    yield return new WaitForFixedUpdate();
                }
            }
            Hit = false;
        }
    }
}
