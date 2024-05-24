using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class DayNightCycle : MonoBehaviour
{

    [Range(0, 24)]
    public float timeOfDay;
    
    [Range (0, 100)]
    public float orbitSpeed;

    public Light sun;

    public Volume skyAndFogVolume;

    private PhysicallyBasedSky skySettings;

    private void Awake()
    {
        skyAndFogVolume.profile.TryGet<PhysicallyBasedSky>(out skySettings);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.F))
        {
            orbitSpeed = 0.1f;
        }
        else
        {
            orbitSpeed = 0.01f;
        }

        timeOfDay += orbitSpeed * Time.deltaTime;

        if(timeOfDay > 24)
        {
            timeOfDay = 0;
        }

        UpdateTime();

    }

    private void OnValidate()
    {
        UpdateTime();
    }

    public void UpdateTime()
    {
        float alpha = timeOfDay / 24f;
        float sunRotation = Mathf.Lerp(-90, 270, alpha);
        sun.transform.rotation = Quaternion.Euler(sunRotation, 0, 0);

        float spaceRotationX = alpha * 360f;
        float spaceRotationZ = alpha * 360f;

        skySettings.spaceRotation.value = new Vector3(spaceRotationX, 0, spaceRotationZ);
    }


}
