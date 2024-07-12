using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CropDetailsList_SO", menuName = "Crop/CropDetailsList_SO")]
public class CropDetailsList_SO : ScriptableObject
{
    public List<CropDetails> cropDetailsList;
}
//this is the real data base class