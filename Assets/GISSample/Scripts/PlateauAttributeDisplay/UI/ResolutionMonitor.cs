using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionMonitor : MonoBehaviour
{
    private int previousWidth;
	private int previousHeight;

	public event Action<int, int> OnResolutionChanged = (int width, int height) => { };

	void Start()
    {
        previousWidth = Screen.width;
		previousHeight = Screen.height;
	}

	// Update is called once per frame
	void Update()
    {
		int currentWidth = Screen.width;
		int currentHeight = Screen.height;
		if( previousWidth != currentWidth || previousHeight != currentHeight)
		{
			OnResolutionChanged(currentWidth, currentHeight);
			previousWidth = currentWidth;
			previousHeight = currentHeight;
		}
	}
}
