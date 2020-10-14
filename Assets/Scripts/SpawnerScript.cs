using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerScript : MonoBehaviour
{
    public GameObject fish;

    float randX;
    float randY;
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;
    public float spawnRate=2f;
    public float nextSpawn=0.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("spawning has started");
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(T)
        if(Time.time > nextSpawn){
            //Debug.Log("time for another");
            nextSpawn = Time.time + spawnRate;
            randX=Random.Range(minX, maxX);
            randY=Random.Range(minY, maxY);
            Vector3 spawnPosition=new Vector3(randX, randY,0);
            Instantiate(fish, spawnPosition, fish.transform.rotation);
            //Debug.Log("Spawning fish at " + randX + " "+randY);
        }
        
    }
}
