using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MouseClick : MonoBehaviour
{
   public CellInfoPanel infoPanel;
   public int CellIndex;

   void OnMouseDown()
   {
        infoPanel.SetCurrentCell(CellIndex);
   }
}
