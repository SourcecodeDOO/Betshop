﻿using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace premier.parser
{
    public delegate void Update(string message, string id);

    public class tipico
    {
        private List<List<string>> upcomingEvents;
        private Dictionary<List<string>, List<bool>> activeEvents;

        private bool htmlReadCompleted;

        Dictionary<header, List<item>> data;

        public bool ReadCompleted { get { return htmlReadCompleted; } }
        public Dictionary<header, List<item>> Data { get { return data; } }

        public tipico() 
        {
            upcomingEvents = new List<List<string>>();
            activeEvents = new Dictionary<List<string>, List<bool>>();

            data = null;

            htmlReadCompleted = false;
        }

        public void PrepareHtml(HtmlDocument document)
        {
            foreach (HtmlElement tr in document.GetElementsByTagName("tr"))
            {
                if (tr.OuterHtml.Trim().StartsWith("<TR class=\"conferenceRow "))
                {
                    string sport = string.Empty;
                    int tipIndex = tr.OuterHtml.IndexOf("<IMG onmouseover=\"tip('");
                    if (tipIndex != -1)
                    {
                        sport = tr.OuterHtml.Substring(tipIndex + 23, tr.OuterHtml.IndexOf('\'', tipIndex + 23) - tipIndex - 23);
                    }

                    List<string> elemets = new List<string>();
                    elemets.Add(sport);
                    elemets.Add("false");
                    elemets.Add("0");
                    elemets.Add("0");
                    foreach (HtmlElement td in tr.GetElementsByTagName("TD"))
                        if (td.GetElementsByTagName("TD").Count == 0)
                            if (td.InnerText == null || td.InnerText.Trim() != "*")
                                elemets.Add(td.InnerText != null ? td.InnerText.Trim() : null);

                    if (elemets.Count == 0 || !elemets[4].Contains(":"))
                    {
                        continue;
                    }

                    upcomingEvents.Add(elemets);
                }
            }

            foreach (HtmlElement tr in document.GetElementsByTagName("tr"))
            {
                if (!string.IsNullOrEmpty(tr.InnerText))
                {
                    string check = tr.OuterHtml.Trim();

                    if (check.StartsWith("<TR style=\"VERTICAL-ALIGN: top\" class=conferenceRow")
                        || check.StartsWith("<TR style=\"VERTICAL-ALIGN: top\" class=\"conferenceRow")
                        || check.StartsWith("<TR style=\"VERTICAL-ALIGN: top")
                        || check.StartsWith("<TR class=conferenceRow>"))
                    {
                        List<string> elemets = new List<string>();

                        foreach (HtmlElement td in tr.GetElementsByTagName("TD"))
                            if (td.GetElementsByTagName("TD").Count == 0)
                                if (td.InnerText == null || td.InnerText.Trim() != "*")
                                    elemets.Add(td.InnerText != null ? td.InnerText.Trim() : null);

                        if (elemets.Count != 0)
                        {
                            activeEvents.Add(elemets, /* buttons */ null);
                        }
                    }
                }
            }

            htmlReadCompleted = true;
        }

        public void ParseData()
        {
            try
            {
                data = new Dictionary<header, List<item>>();

                header activeHeader = new header("Live matches coming up");

                activeHeader.Add("Sport");
                activeHeader.Add("Started");
                activeHeader.Add("Home team score");
                activeHeader.Add("Away team score");
                activeHeader.Add("Time");
                activeHeader.Add("Home team");
                activeHeader.Add("Away team");
                activeHeader.Add("Match odds 1");
                activeHeader.Add("Match odds x");
                activeHeader.Add("Match odds 2");
                activeHeader.Add("HT/Set/Third 1");
                activeHeader.Add("HT/Set/Third X");
                activeHeader.Add("HT/Set/Third 2");
                activeHeader.Add("Over / Under +/-");
                activeHeader.Add("Over / Under +/- +");
                activeHeader.Add("Over / Under +/- -");
                activeHeader.Add("Suspicious");

                data.Add(activeHeader, new List<item>());

                foreach (List<string> elemets in upcomingEvents)
                {
                    // time fix
                    if (elemets[4].IndexOf(':') != -1)
                    {
                        elemets[4] = elemets[4].Substring(elemets[4].IndexOf(':') - 2);
                    }
                    // remove this element
                    if (elemets.Count > 6)
                    {
                        elemets.RemoveAt(6);
                    }

                    item item = new item(activeHeader);

                    for (int counter = 0; counter < activeHeader.Lenght; counter++)
                    {
                        item.AddString(counter < elemets.Count ? elemets[counter] : string.Empty);
                    }

                    data[activeHeader].Add(item);
                }

                activeHeader = null;
                item currentItem = null;

                foreach (List<string> elemets in activeEvents.Keys)
                {
                    if (elemets.Count == 36)
                    {
                        activeHeader = new header(elemets[7]);

                        activeHeader.Add(elemets[1]);
                        activeHeader.Add("Home team");
                        activeHeader.Add("Home team score");
                        activeHeader.Add("Away team score");
                        activeHeader.Add("Away team");

                        if (!string.IsNullOrEmpty(elemets[13]))
                        {
                            activeHeader.Add(elemets[13] + " " + elemets[15]);
                            activeHeader.Add(elemets[13] + " " + elemets[16]);
                            activeHeader.Add(elemets[13] + " " + elemets[17]);
                        }
                        if (!string.IsNullOrEmpty(elemets[19]))
                        {
                            activeHeader.Add(elemets[19] + " " + elemets[21]);
                            activeHeader.Add(elemets[19] + " " + elemets[22]);
                            activeHeader.Add(elemets[19] + " " + elemets[23]);
                        }
                        if (!string.IsNullOrEmpty(elemets[25]))
                        {
                            if (elemets[25] == "Set score")
                            {
                                activeHeader.Add(elemets[25] + " home");
                                activeHeader.Add(elemets[25]);
                                activeHeader.Add(elemets[25] + " away");
                            }
                            else
                            {
                                activeHeader.Add(elemets[25] + " " + elemets[27]);
                                activeHeader.Add(elemets[25] + " " + elemets[28]);
                                activeHeader.Add(elemets[25] + " " + elemets[29]);
                            }
                        }
                        if (!string.IsNullOrEmpty(elemets[31]))
                        {
                            activeHeader.Add(elemets[31] + " " + elemets[33]);
                            activeHeader.Add(elemets[31] + " " + elemets[34]);
                            activeHeader.Add(elemets[31] + " " + elemets[35]);
                        }

                        if (!data.ContainsKey(activeHeader))
                        {
                            activeHeader.Add("Started");
                            activeHeader.Add("Suspicious");
                            data.Add(activeHeader, new List<item>());
                        }
                    }
                    else if (elemets.Contains("1st half") || elemets.Contains("1. Half"))
                    {
                        // List<bool> buttons = activeEvents[elemets];
                        while (elemets[0] != "1st half" && elemets[0] != "1. Half")
                        {
                            elemets.RemoveAt(0);
                            // buttons.RemoveAt(0);
                        }
                        elemets.RemoveAt(0);
                        // buttons.RemoveAt(0);

                        item firstHalf = new item(activeHeader);
                        currentItem.FirstHalf = firstHalf;

                        int counter = 0;
                        while (!activeHeader[counter].StartsWith("Match"))
                        {
                            firstHalf.AddString(currentItem[counter]);
                            counter++;
                        }

                        int added = 0;
                        for (int i = 0; i < activeHeader.Lenght - counter; i++)
                        {
                            if (elemets.Count != 0)
                            {
                                if (string.IsNullOrEmpty(elemets[0]) && (activeHeader[firstHalf.Lenght].EndsWith("1") || activeHeader[firstHalf.Lenght].EndsWith("+/- "))) // if (string.IsNullOrEmpty(elemets[0].Trim())) // nije dugme i ne sadrzi nikakvu vrednost
                                {
                                    firstHalf.AddString(string.Empty);
                                    firstHalf.AddString(string.Empty);
                                    firstHalf.AddString(string.Empty);

                                    i += 2;
                                }
                                else
                                {
                                    firstHalf.AddString(elemets[0]);
                                }

                                elemets.RemoveAt(0);
                            }
                            else
                            {
                                firstHalf.AddString(string.Empty);
                                added++;
                            }
                        }
                        if (elemets.Count != 0 || added != 2)
                        {
                            firstHalf[firstHalf.Lenght - 1] = "true";
                        }
                        else
                        {
                            firstHalf[firstHalf.Lenght - 1] = "false";
                        }
                    }
                    else if (activeHeader != null && elemets.Count >= 7)
                    {
                        //List<string> originalElements = new List<string>();
                        //originalElements.AddRange(elemets);

                        if (elemets.Contains(":"))
                        {
                            elemets.Remove(":");
                        }
                        bool halftimebreak = false;
                        if (elemets.Contains("Only during halftime break"))
                        {
                            elemets.Remove("Only during halftime break");
                            halftimebreak = true;
                        }

                        if (elemets.Count > 0 && elemets[elemets.Count - 1].Contains("further bets"))
                        {
                            elemets.RemoveAt(elemets.Count - 1);
                        }

                        item item = new item(activeHeader);

                        int added = 0;
                        for (int counter = 0; counter < activeHeader.Lenght; counter++)
                        {
                            if (elemets.Count != 0 && string.IsNullOrEmpty(elemets[0]) && (activeHeader[counter] == "Sets overall +/- " || activeHeader[counter] == "Set score home")) 
                            {
                                elemets.RemoveAt(0);
                            }

                            if (elemets.Count != 0)
                            {
                                if (string.IsNullOrEmpty(elemets[0]) && (activeHeader[item.Lenght].EndsWith("1") || activeHeader[item.Lenght].EndsWith("+/- ") || activeHeader[item.Lenght].EndsWith("cap "))/* && activeHeader.Event.ToLower().Contains("football") */ )
                                {
                                    item.AddString(string.Empty);
                                    item.AddString(string.Empty);
                                    item.AddString(string.Empty);

                                    counter += 2;
                                }
                                else
                                {
                                    item.AddString(elemets[0]);
                                }

                                elemets.RemoveAt(0);
                            }
                            else
                            {
                                item.AddString(string.Empty);
                                added++;
                            }
                        }

                        if (activeHeader.Event.ToLower().Contains("tennis"))
                        {
                            if (item.Lenght > 11 && item[11].Trim().Length > 1 && item[11].Trim().EndsWith("6"))
                            {
                                item.OldData[11] = item[11] = "(" + item[11].Trim().Substring(0, item[11].Trim().Length - 1) + ") 6";
                            }
                            if (item.Lenght > 13 && item[13].Trim().Length > 1 && item[13].Trim().StartsWith("6"))
                            {
                                item.OldData[13] = item[13] = "6 (" + item[13].Trim().Substring(1) + ")";
                            }
                        }

                        item[item.Lenght - 2] = "true";

                        if (!halftimebreak && (elemets.Count != 0 || added != 2))
                        {
                            item[item.Lenght - 1] = "true";
                        }
                        else
                        {
                            item[item.Lenght - 1] = "false";
                        }

                        data[activeHeader].Add(item);

                        currentItem = item;
                    }
                    else
                    {
                        Console.WriteLine("!(activeHeader != null && elemets.Count >= 7)");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }


        Dictionary<string, string> listOfIds;
        string token;
        Update sendUpdate;
        Dictionary<header, List<item>> itemCatalog; 

        public void InitAll(Dictionary<header, List<item>> itemCatalog, Update sendUpdate, string token, Dictionary<string, string> listOfIds)
        {
            this.itemCatalog = itemCatalog;
            this.sendUpdate = sendUpdate;
            this.token = token;
            this.listOfIds = listOfIds;
        }

        public void DoAll()
        {
            ParseData();
            CompareData();
        }

        public void CompareData()
        {
            List<item> allItems = new List<item>();

            foreach (header h in itemCatalog.Keys)
            {
                allItems.AddRange(itemCatalog[h]);
            }

            foreach (header h in data.Keys)
            {
                if (!itemCatalog.ContainsKey(h))
                {
                    itemCatalog.Add(h, new List<item>());
                }

                List<item> items = itemCatalog[h];

                foreach (item newItem in data[h])
                {
                    bool found = false;
                    foreach (item oldItem in items)
                    {
                        if (oldItem.IsSameEvent(newItem))
                        {
                            found = true;

                            string update = oldItem.Update(newItem, token);
                            if (!string.IsNullOrEmpty(oldItem.Id) && !string.IsNullOrEmpty(update))
                            {
                                sendUpdate(update, oldItem.Id);
                            }

                            if (allItems.Contains(oldItem))
                            {
                                allItems.Remove(oldItem);
                            }

                            break;
                        }
                    }

                    if (!found)
                    {
                        items.Add(newItem);

                        if (listOfIds.ContainsValue(newItem.ToString()))
                        {
                            foreach (string id in listOfIds.Keys)
                            {
                                if (newItem.ToString() == listOfIds[id])
                                {
                                    newItem.Id = id;
                                    sendUpdate(newItem.JSON(token), newItem.Id);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            foreach (item i in allItems)
            {
                if (itemCatalog.ContainsKey(i.Header) && itemCatalog[i.Header].Contains(i))
                {
                    itemCatalog[i.Header].Remove(i);
                    if (!string.IsNullOrEmpty(i.Id))
                        sendUpdate(i.Removed(token), i.Id);
                }
            }
        }

        // *** 
        // not in use !!!
        public static Dictionary<header, List<item>> ParseHtml(HtmlDocument document)
        {
            Dictionary<header, List<item>> itemCatalog = new Dictionary<header, List<item>>();

            header activeHeader = new header("Live matches coming up");

            activeHeader.Add("Time");
            activeHeader.Add("Home team");
            activeHeader.Add("Away team");
            activeHeader.Add("Match odds 1");
            activeHeader.Add("Match odds x");
            activeHeader.Add("Match odds 2");
            activeHeader.Add("HT/Set/Third 1");
            activeHeader.Add("HT/Set/Third X");
            activeHeader.Add("HT/Set/Third 2");
            activeHeader.Add("Over / Under +/-");
            activeHeader.Add("Over / Under +/- +");
            activeHeader.Add("Over / Under +/- -");

            itemCatalog.Add(activeHeader, new List<item>());

            foreach (HtmlElement tr in document.GetElementsByTagName("tr"))
            {
                if (tr.OuterHtml.Trim().StartsWith("<TR class=\"conferenceRow "))
                {
                    List<string> elemets = new List<string>();
                    foreach (HtmlElement td in tr.GetElementsByTagName("TD"))
                    {
                        if (td.GetElementsByTagName("TD").Count == 0)
                        {
                            elemets.Add(td.InnerText != null ? td.InnerText.Trim() : null);
                        }
                    }

                    if (elemets.Count == 0 || !elemets[0].Contains(":"))
                    {
                        continue;
                    }

                    // time fix
                    if (elemets[0].IndexOf(':') != -1)
                    {
                        elemets[0] = elemets[0].Substring(elemets[0].IndexOf(':') - 2);
                    }
                    // remove this element
                    if (elemets.Count > 2)
                    {
                        elemets.RemoveAt(2);
                    }

                    item item = new item(activeHeader);

                    for (int counter = 0; counter < activeHeader.Lenght; counter++)
                    {
                        item.AddString(counter < elemets.Count ? elemets[counter] : string.Empty);
                    }

                    itemCatalog[activeHeader].Add(item);
                }
            }

            activeHeader = null;

            item currentItem = null;

            foreach (HtmlElement tr in document.GetElementsByTagName("tr"))
            {
                if (!string.IsNullOrEmpty(tr.InnerText))
                {
                    string check = tr.OuterHtml.Trim();

                    if (check.StartsWith("<TR style=\"VERTICAL-ALIGN: top\" class=conferenceRow")
                        || check.StartsWith("<TR style=\"VERTICAL-ALIGN: top\" class=\"conferenceRow")
                        || check.StartsWith("<TR style=\"VERTICAL-ALIGN: top")
                        || check.StartsWith("<TR class=conferenceRow>"))
                    {
                        List<string> elemets = new List<string>();
                        foreach (HtmlElement td in tr.GetElementsByTagName("TD"))
                        {
                            if (td.GetElementsByTagName("TD").Count == 0)
                            {
                                elemets.Add(td.InnerText != null ? td.InnerText.Trim() : null);
                            }
                        }

                        if (elemets.Count != 0)
                        {
                            if (elemets.Count == 36)
                            {
                                activeHeader = new header(elemets[7]);

                                activeHeader.Add(elemets[1]);
                                activeHeader.Add("Home team");
                                activeHeader.Add("Home team score");
                                activeHeader.Add("Away team score");
                                activeHeader.Add("Away team");

                                if (!string.IsNullOrEmpty(elemets[13]))
                                {
                                    activeHeader.Add(elemets[13] + " " + elemets[15]);
                                    activeHeader.Add(elemets[13] + " " + elemets[16]);
                                    activeHeader.Add(elemets[13] + " " + elemets[17]);
                                }
                                if (!string.IsNullOrEmpty(elemets[19]))
                                {
                                    activeHeader.Add(elemets[19] + " " + elemets[21]);
                                    activeHeader.Add(elemets[19] + " " + elemets[22]);
                                    activeHeader.Add(elemets[19] + " " + elemets[23]);
                                }
                                if (!string.IsNullOrEmpty(elemets[25]))
                                {
                                    activeHeader.Add(elemets[25] + " " + elemets[27]);
                                    activeHeader.Add(elemets[25] + " " + elemets[28]);
                                    activeHeader.Add(elemets[25] + " " + elemets[29]);
                                }
                                if (!string.IsNullOrEmpty(elemets[31]))
                                {
                                    activeHeader.Add(elemets[31] + " " + elemets[33]);
                                    activeHeader.Add(elemets[31] + " " + elemets[34]);
                                    activeHeader.Add(elemets[31] + " " + elemets[35]);
                                }

                                if (!itemCatalog.ContainsKey(activeHeader))
                                {
                                    itemCatalog.Add(activeHeader, new List<item>());
                                }
                            }
                            else if (elemets.Contains("1st half"))
                            {
                                while (elemets[0] != "1st half")
                                {
                                    elemets.RemoveAt(0);
                                }
                                elemets.RemoveAt(0);

                                item firstHalf = new item(activeHeader);
                                currentItem.FirstHalf = firstHalf;

                                int counter = 0;
                                while (!activeHeader[counter].StartsWith("Match"))
                                {
                                    firstHalf.AddString(currentItem[counter]);
                                    counter++;
                                }
                                for (int i = 0; i < activeHeader.Lenght - counter; i++)
                                {
                                    firstHalf.AddString(i < elemets.Count ? elemets[i] : string.Empty);
                                }
                            }
                            else if (activeHeader != null && elemets.Count >= 7)
                            {
                                elemets.Remove(":");
                                elemets.Remove("Only during halftime break");
                                // izbacujemo poslednji element
                                // ovo je 'additionall bets' za 'top match'
                                if (elemets.Count == activeHeader.Lenght + 1)
                                {
                                    elemets.RemoveAt(elemets.Count - 1);
                                }
                                // ovo jos treba videti sta i kako... 
                                else if (elemets.Count == activeHeader.Lenght + 2 && activeHeader.Event == "Tennis")
                                {
                                    elemets.RemoveAt(15);
                                    elemets.RemoveAt(11);
                                }

                                item item = new item(activeHeader);

                                if (elemets.Count != activeHeader.Lenght)
                                {
                                    // analyse this
                                    // Console.WriteLine("elemets.Count != activeHeader.Lenght");
                                }

                                for (int counter = 0; counter < activeHeader.Lenght; counter++)
                                {
                                    item.AddString(counter < elemets.Count ? elemets[counter] : string.Empty);
                                }

                                itemCatalog[activeHeader].Add(item);

                                currentItem = item;
                            }
                            else
                            {
                                Console.WriteLine("!(activeHeader != null && elemets.Count >= 7)");
                            }
                        }
                    }
                }
            }

            return itemCatalog;
        }
    }
}
