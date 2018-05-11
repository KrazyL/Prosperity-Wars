﻿using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Nashet.Utils;
using UnityEngine.EventSystems;
//namespace Nashet.EconomicSimulation
//{
using Nashet.EconomicSimulation;

public class SelectionComponent : MonoBehaviour
{
    bool isSelecting = false;
    Vector3 mousePosition1;

    //public GameObject selectionCirclePrefab;
    private static Camera camera; // it's OK
    [SerializeField]
    private KeyCode AdditionKey;

    private void Start()
    {
        camera = GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())//!hovering over UI) 
            {
                SelectUnitOrProvince();
            }
            if (isSelecting)
                EndFrameSelection();// If we let go of the left mouse button, end selection
        }
        else
        {
            // If we press the left mouse button, begin selection and remember the location of the mouse
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                StartFrameSelection();
            }
        }

        if (!Game.selectedUnits.IsEmpty() && Input.GetMouseButtonDown(1)) // MOUSE RIGHT BUTTON
        {
            int meshNumber = Province.FindByCollider(SelectionComponent.getRayCastMeshNumber());
            if (meshNumber > 0) // send armies to another province
                Game.selectedUnits.PerformAction(x => x.SendTo(World.FindProvince(meshNumber)));
        }
        if (Input.GetKeyDown(KeyCode.Return)) // enter key
            MainCamera.Get.closeToppestPanel();

        Game.previoslySelectedProvince = Game.selectedProvince;
        // Highlight all objects within the selection box
        //if (isSelecting)
        //{
        //    foreach (var selectableObject in FindObjectsOfType<SelectableUnitComponent>())
        //    {
        //        if (IsWithinSelectionBounds(selectableObject.gameObject))
        //        {
        //            if (selectableObject.selectionCircle == null)
        //            {
        //                selectableObject.selectionCircle = Instantiate(selectionCirclePrefab);
        //                selectableObject.selectionCircle.transform.SetParent(selectableObject.transform, false);
        //                selectableObject.selectionCircle.transform.eulerAngles = new Vector3(90, 0, 0);
        //            }
        //        }
        //        else
        //        {
        //            if (selectableObject.selectionCircle != null)
        //            {
        //                Destroy(selectableObject.selectionCircle.gameObject);
        //                selectableObject.selectionCircle = null;
        //            }
        //        }
        //    }
        //}
    }

    private void StartFrameSelection()
    {
        isSelecting = true;
        mousePosition1 = Input.mousePosition;
    }

    private void EndFrameSelection()
    {
        if (mousePosition1 != Input.mousePosition)
        {
            if (!Input.GetKey(AdditionKey))
                Game.selectedUnits.ToList().PerformAction(x => x.DeSelect());
            foreach (var selectableObject in FindObjectsOfType<Unit>())
            {
                if (IsWithinSelectionBounds(selectableObject.gameObject))
                {
                    selectableObject.GetComponent<Unit>().Select();
                }
            }

            //var sb = new StringBuilder();
            //sb.AppendLine(string.Format("Selecting [{0}] Units", selectedObjects.Count));
            //foreach (var selectedObject in selectedObjects)
            //    sb.AppendLine("-> " + selectedObject.gameObject.name);
            //Debug.Log(sb.ToString());
        }
        isSelecting = false;
    }

    private void SelectUnitOrProvince()
    {
        var collider = getRayCastMeshNumber();
        if (collider != null)
        {
            int provinceNumber = Province.FindByCollider(collider);
            if (provinceNumber > 0)
            {
                MainCamera.selectProvince(provinceNumber);
                if (!Input.GetKey(AdditionKey)) // don't de select units if shift is pressed
                    Game.selectedUnits.ToList().PerformAction(x => x.DeSelect());
            }
            else
            {
                var unit = collider.transform.GetComponent<Unit>();
                if (unit != null)
                {
                    if (Input.GetKey(AdditionKey))
                    {
                        if (Game.selectedUnits.Contains(unit))
                            unit.DeSelect();
                        else
                            unit.Select();
                    }
                    else
                    {
                        if (Game.selectedUnits.Count > 0)
                        {
                            Game.selectedUnits.ToList().PerformAction(x => x.DeSelect());
                        }
                        unit.Select();
                    }
                }
            }
        }
        else
        {
            MainCamera.selectProvince(-1);
            if (!Input.GetKey(AdditionKey))
                Game.selectedUnits.ToList().PerformAction(x => x.DeSelect());
        }
    }
    public bool IsWithinSelectionBounds(GameObject gameObject)
    {
        if (!isSelecting)
            return false;

        var camera = Camera.main;
        var viewportBounds = Utils.GetViewportBounds(camera, mousePosition1, Input.mousePosition);
        return viewportBounds.Contains(camera.WorldToViewportPoint(gameObject.transform.position));
    }

    void OnGUI()
    {
        if (isSelecting)
        {
            // Create a rect from both mouse positions
            var rect = Utils.GetScreenRect(mousePosition1, Input.mousePosition);
            Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            Utils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));

            //// Left example
            //Utils.DrawScreenRectBorder(new Rect(32, 32, 256, 128), 2, Color.green);
            //// Right example
            //Utils.DrawScreenRect(new Rect(320, 32, 256, 128), new Color(0.8f, 0.8f, 0.95f, 0.25f));
            //Utils.DrawScreenRectBorder(new Rect(320, 32, 256, 128), 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }
    // remake it to return mesh collider, on which will be chosen object
    public static Collider getRayCastMeshNumber()
    {
        RaycastHit hit;

        if (EventSystem.current.IsPointerOverGameObject())
            return null;// -3; //hovering over UI
        else
        {
            if (!Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit))
                return null;// -1;
        }
        return hit.collider;
    }
}
//}