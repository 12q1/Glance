//Glance Inventory Viewer
//Release Date: 23/06/2020
//Compiled for Space Engineers v 1.194.211
public class itemObject
{
    public string Name {get; set;}
    public string Type {get; set;}
    public float Quantity {get; set;}
    public itemObject(string name, string type, float quantity)
    {
        Name = name;
        Type = type;
        Quantity = quantity;
    }
}
//itemObjects are pushed into the allItems list for further processing and render
//also recycled in totals lists

public static string SplitCamelCase(string input)
{
    return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
}
//Credit to Tillito for this function
//https://stackoverflow.com/questions/773303/splitting-camelcase

public static string GenerateCell(itemObject input, bool rightMost)
{
    int maxWidth = 33;
    string result = "";
    string splitName = SplitCamelCase(input.Name);
    if(input.Name.Contains("NATO"))
    {
        if(input.Name.Contains("5p"))
        {
            splitName = "NATO 56x45mm";
        }
        else
        {
            splitName = "NATO 25x184mm";
        }
    }
    int nameLength = splitName.Length;
    if(input.Type == "Ship")
    {
        nameLength = input.Name.Length;
        if(input.Quantity > 0.99 || input.Quantity != input.Quantity) //handle NaN and if a ship is over 99% then consider it full
        {
            input.Quantity = 100;
        }
        else
        {
            input.Quantity = input.Quantity * 100;
        }
    } 
    //dirty hack but ships already have spaces in their name so we just keep their original name
    //TODO this whole section is in dire need of refactor
    int quantityLength = input.Quantity.ToString().Length;
    int numberOfDashes = maxWidth - (nameLength + quantityLength) - 1;
    string dashes = new string('─', numberOfDashes);
    if(input.Type == "Ore" || input.Type == "Ingot" || input.Type == "Gas" || input.Type == "Power" || input.Type == "Ship")
    //Need to handle long numbers differently than normal items/components
    //also need to recalculate string format
    {
        string doubleQuantity = Math.Round(Convert.ToDouble(input.Quantity), 2).ToString("0.00");
        if(input.Type == "Power")
        {
            doubleQuantity = Convert.ToSingle(Math.Round(input.Quantity, 2)).ToString("0.00");
            //doubleQuantity = doubleQuantity.Format("0.00");
        }
        if(input.Type == "Gas")
        {
            doubleQuantity = Math.Round(Convert.ToDouble(input.Quantity), 2).ToString();
        }
        if(input.Type == "Ship")
        {
            doubleQuantity = Convert.ToSingle(Math.Round(input.Quantity, 2)).ToString();
        }
        quantityLength = doubleQuantity.Length;
        numberOfDashes = 33 - (nameLength + quantityLength) - 7;
        dashes = new string('─', numberOfDashes);
        switch(input.Type)
        {
            case "Gas":
                result += " " + splitName + dashes + "─────" + doubleQuantity + " L ";
                break;
            case "Power":
                if(input.Name == "BatteryStored" || input.Name == "BatteryCapacity")
                {
                    result += " " + splitName + dashes + "─" + doubleQuantity + " MWh ";
                }
                else
                {
                    result += " " + splitName + dashes + "──" + doubleQuantity + " MW ";
                }
                break;
            case "Ship":
                result += "" + input.Name + dashes + "PWR " + doubleQuantity + "% ";
                break;
            default:
                result += " " + splitName + dashes + "────" + doubleQuantity + " kg ";
                break;
        }
    }     
    else 
    {
        result += splitName + dashes + input.Quantity + " ";
    }

    //if its a rightmost column we slap a new line onto it
    if(rightMost)
    {
        
        result +="\n";
        return result;
    }
    else
    {
    //we slap a | on it
        result += "│";
    }

    return result;
}

public static string Display(List<itemObject> Components, List<itemObject> Ingots, List<itemObject> Ores, List<itemObject> Misc, List<itemObject> Gas, List<itemObject> Power, List<itemObject> Ships)
{
    //Abandon all hope ye who enter this forest of ifs
    int maxHeight = 33;
    string result = "            COMPONENTS           │             POWER               │              INGOTS               \n";
    int i;
    for (i= 0; i < maxHeight; i++)
    //render each line by pulling respective index position from each list
    {
        //handle column 0 (Read from bottom up)
        if(i >= Components.Count)
        {
            //if i is greater than the length of the components list we know all components are printed
            //add one empty slot
            if(i == Components.Count || i>= Components.Count+Misc.Count+2)
            {
                result = result + "                                 │";
            }
            else
            //and push Misc items into column 0
            {
                if(i == Components.Count+1)
                {
                    if(Misc.Count == 0){
                        result += "                                 │";
                    }
                    else
                    {
                        result += "               MISC              │";
                    }
                }
                else
                {
                    int iMisc = i - (Components.Count+2);
                    result = result + GenerateCell(Misc[iMisc], false);
                }
            }
        }
        else
        {
            result = result + GenerateCell(Components[i], false);
            //push component items into column 0
        }
        
        //handle column 1 (Read from bottom up)
        if(i >= Power.Count)
        {
            if(i == Power.Count || i>= Power.Count+Ships.Count+2)
            {
                result += "                                 │";
            }
            else
            //and push ship items into column 1
            {
                if(i == Power.Count+1)
                {
                    if(Ships.Count == 0)
                    {
                        result += "                                 │";
                    }
                    else
                    {
                        result += "             SHIPS               │";
                    }
                }
                else
                {
                    int iShip = i - (Power.Count+2);
                    result = result + " " + GenerateCell(Ships[iShip], false);
                    //push ship items into column 1
                }
            }

        }
        else
        {
            result = result + GenerateCell(Power[i], false);
            //push power items into column 1
        }

        //handle column 2 (Read from bottom up)
        if(i >= Ingots.Count)
        {
            if(i == Ingots.Count || i>= Ingots.Count+Ores.Count+2)
            {
                if(i == Ingots.Count + Ores.Count + 3)
                {
                    if(Gas.Count >0)//Hide the title if there's none
                    {
                        result += "               GAS                 \n";
                    }
                    else
                    {
                        result += "                                 \n";
                    }
                } 
                else
                {
                    if(i>= Ingots.Count + Ores.Count + Gas.Count + 4 || i == Ingots.Count || i== Ingots.Count+Ores.Count+2)
                    {
                        result = result + "                                 \n";
                    }
                    else
                    {
                        //push Gas items into this column 2
                        int iGas = i - (Ingots.Count + Ores.Count + 4);
                        result = result + GenerateCell(Gas[iGas], true);
                    }
                }
            }
            else
            //and push Ore items into this column 2 
            {
                if(i == Ingots.Count+1)
                {
                    if(Ores.Count > 0)//Hide the title if there's none
                    {
                        result += "               ORES                \n";
                    }
                    else
                    {
                        result += "                                 \n";
                    }
                }
                else
                {
                    int iOre = i - (Ingots.Count+2);
                    result = result + GenerateCell(Ores[iOre], true);

                }
            }
        }
        else
        {
            result = result + GenerateCell(Ingots[i], true);
            //push ingot items into column 2
        }
    }
    return result;
}


public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
    //update script every 100 ticks
    //TODO check if there is a way to reduce cycles 
}

public void Save()
{

}

public void Main(string argument, UpdateType updateSource)
{
    //declare a list of all items so we can add items later
    List<itemObject> allItems = new List<itemObject>();
    
    //Sets up lcd panel as display and rudimentary console
    var console = GridTerminalSystem.GetBlockWithName("MainLCD") as IMyTextPanel;
    if(console != null)
    {
        console.SetValueFloat("FontSize", 0.5f); //set fontsize to halfsize
        console.SetValue<long>("Font", 1147350002); //set to monospace font
        console.SetValue("FontColor",  new Color(/*Red*/220,  /*Green*/255, /*Blue*/255));  //set text color to bluish white
        console.SetValue("BackgroundColor", new Color(/*Red*/0,  /*Green*/40, /*Blue*/75)); //set background to cyanish blue
        console.WriteText("", false); //clear screen

        //screenwidth of WideLCD at this font size and default padding is roughly 102 monospace chars and 34 lines tall
    }
    else
    {
        throw new Exception ("Unable to detect LCD screen, check that your target LCD panel is named MainLCD");
    }

    //Get a list of all items in all inventories on the grid
    //Credit and thanks goes to Pembroke for the logic to loop through all inventories and access items
    //https://steamcommunity.com/app/244850/discussions/0/1640918469756217393
    int ixBlock, ixInventory, ixItem;
    IMyInventory inventory;
    MyInventoryItem item;
    List<MyInventoryItem> lstItems;
    List<IMyTerminalBlock> lstBlocks = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocks(lstBlocks);
    for (ixBlock = 0; ixBlock < lstBlocks.Count; ixBlock++)
    //loop through all the blocks in the grid
    {
        if (lstBlocks[ixBlock].HasInventory)
        //check if block has an inventory
        {
            for (ixInventory = 0; ixInventory < lstBlocks[ixBlock].InventoryCount; ixInventory++)
            //loop through all inventories of the block (some blocks have more than 1 inventory)
            {
                inventory = lstBlocks[ixBlock].GetInventory(ixInventory);
                lstItems = new List<MyInventoryItem>();
                inventory.GetItems(lstItems);
                for (ixItem = 0; ixItem < lstItems.Count; ixItem++)
                //loop through all items within an inventory
                {
                    item = lstItems[ixItem];
                    // ... add code here, that is the "item" refers to a single item, so display it's name, or add its amount to some variable and show a summary at the end of your code, or whatever it is you want to do with the items...
                    string typeString = item.Type.ToString();

                    var splitString = typeString.Split(new[]{'_','/'});
                    string itemName = splitString[2];
                    string itemType = splitString[1];
                    //console.WriteText(itemType);
                    float itemQuantity;
                    if(itemType == "Ingot" || itemType == "Ore")
                    {
                        itemQuantity = (float)item.Amount.RawValue / 1000000f;
                        //if item is an ingot we need to use rawvalue and convert the long number 
                        //https://forum.keenswh.com/threads/problems-pulling-item-quantities-from-inventories.7228634/
                        if(itemName == "Stone" && itemType == "Ingot")
                        {
                            itemName = "Gravel";
                            //handle edge case stone ingots are actually gravel
                        }
                    }
                    else
                    {
                        itemQuantity = float.Parse(item.Amount.ToString());
                        //otherwise we just parse the number from the string
                        //removing this check causes non-ingot items to appear as 0
                    }
                    if (typeString.Contains("NATO"))
                    {
                        itemName = splitString[2] + splitString[3];
                        //console.WriteText(itemName + "\n", true);
                    }

                    allItems.Add(new itemObject(itemName, itemType, itemQuantity));
                    //push item to allItems for further processing 
                }
            }
        }
    }
    //retrieve gas info
    //thanks to Pasukaru for snippets
    //https://github.com/Pasukaru/SpaceEngineers-InGame-Scripts/blob/master/OxygenLevelDisplay.cs
    List<IMyTerminalBlock> tanks= new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyGasTank>(tanks);
    if (tanks.Count > 0) 
    { 
        for (int i = 0; i < tanks.Count; i++)
        { 
            var tank = (tanks[i] as IMyGasTank);
            float amount = Convert.ToSingle(tank.Capacity * tank.FilledRatio);
            if(tank.ToString().ToLower().Contains("hydrogen")){
                allItems.Add(new itemObject("Hydrogen", "Gas", amount));
            } 
            else
            {
                allItems.Add(new itemObject("Oxygen", "Gas", amount));
            } 
        }
    }

    //retrieve battery info
    List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries);
    if(batteries.Count > 0)
    {
        for (int i = 0; i < batteries.Count; i++)
        {
            var battery = (batteries[i] as IMyBatteryBlock);
            allItems.Add(new itemObject("CurrentInput", "Power", battery.CurrentInput));
            //allItems.Add(new itemObject("CurrentOutput(-)", "Power", battery.CurrentOutput));
            allItems.Add(new itemObject("BatteryStored", "Power", battery.CurrentStoredPower));
            allItems.Add(new itemObject("BatteryCapacity", "Power", battery.MaxStoredPower));
        }
    }

    //power producers
    List<itemObject> powerProducers = new List<itemObject>();
    List<IMyPowerProducer> generators = new List<IMyPowerProducer>();
    GridTerminalSystem.GetBlocksOfType<IMyPowerProducer>(generators);
    if(generators.Count > 0)
    {
        for(int i = 0; i < generators.Count; i++)
        {
            var generator = (generators[i] as IMyPowerProducer);
            allItems.Add(new itemObject("CurrentOutput", "Power", generator.CurrentOutput));
            allItems.Add(new itemObject("MaxOutput", "Power", generator.MaxOutput));
            //I had a pretty looking switch here but it was hardcoded to specific vanilla blocktypes
            //Need to use ifs because can't use Contains on a switch
            string blockDef = generator.BlockDefinition.ToString().ToLower();
            if(blockDef.Contains("turbine"))
            {
                powerProducers.Add(new itemObject(">Wind", "Power", generator.CurrentOutput));
            }
            if(blockDef.Contains("hydrogenengine"))
            {
                powerProducers.Add(new itemObject(">Engines", "Power", generator.CurrentOutput));
                //TODO parse detailedinfo for the fuel Capacity / FilledRatio
                //console.WriteText(generator.DetailedInfo.ToString() + "\n", true);
                //var tank = generator as IMyHydrogenEngine;
                //allItems.Add(new itemObject("Hydrogen", "Gas", Convert.ToSingle(tank.Capacity * tank.FilledRatio)));
            }
            if(blockDef.Contains("reactor"))
            {
                powerProducers.Add(new itemObject(">Reactors", "Power", generator.CurrentOutput));
            }
            if(blockDef.Contains("solar"))
            {
                powerProducers.Add(new itemObject(">Solar", "Power", generator.CurrentOutput));
            }
            if(blockDef.Contains("battery"))
            {
                powerProducers.Add(new itemObject(">Batteries", "Power", generator.CurrentOutput));
            }
            //console.WriteText(generator.BlockDefinition.ToString() + "\n", true);
        }
    }

    //retrieve grid info
    string stationGrid = console.CubeGrid.ToString();
    //console.WriteText(stationGrid);
    List<IMyShipConnector> connectors = new List<IMyShipConnector>();
    GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(connectors);
    if(connectors.Count > 0)
    {
        for (int i = 0; i < connectors.Count; i++)
        {
            var connector = (connectors[i] as IMyShipConnector);
            if(connector.CubeGrid != console.CubeGrid){
                //if CubeGrid is anything other than the station grid
                //fetch a list of the batteries of that grid
                float MaxStoredPower = 0f;
                float CurrentStoredPower = 0f;
                IEnumerable<IMyBatteryBlock> gridBatteries = batteries.Where(x => x.CubeGrid == connectors[i].CubeGrid);
                gridBatteries.ToList().ForEach(j => 
                {
                    MaxStoredPower += j.MaxStoredPower;
                    CurrentStoredPower += j.CurrentStoredPower;
                });
                float battPercentage = CurrentStoredPower/MaxStoredPower;
                //console.WriteText(battPercentage.ToString(), true);
                string nameBuilder = connector.CubeGrid.CustomName.ToString();
                if(nameBuilder.Length > 20)
                {
                    nameBuilder = nameBuilder.Substring(0,20);
                }

                allItems.Add(new itemObject(nameBuilder, "Ship", battPercentage));
                //console.WriteText(connector.CubeGrid.CustomName.ToString() + "\n", true);
            }
        }
    }


    //loop through allItems list and reduce to totals
    //https://stackoverflow.com/questions/47790257/combine-duplicates-in-a-list
    //need to group with composite key to retain name and type
    //https://stackoverflow.com/questions/13400453/linq-groupby-whilst-keeping-all-object-fields

    var totals = allItems.GroupBy(x => new {x.Name, x.Type},
         (key, values) => new {
            Name = key.Name,
            Type = key.Type,
            Quantity = values.Sum(x => x.Quantity)
         });
    
    var orderedTotals = totals.OrderBy(s => s.Name);

    var totalPower = powerProducers.GroupBy(x => new {x.Name, x.Type},
         (key, values) => new {
            Name = key.Name,
            Type = key.Type,
            Quantity = values.Sum(x => x.Quantity)
         });

    var orderedPowerProducers = totalPower.OrderBy(s => s.Name);
    //sort it alphabetically here so later lists are also alphabetical without needing to call sort or cast to new lists

    //declaring multiple lists to store item types separately
    //not sure if this is performance friendly but done for convenience and readability
    List<itemObject> components = new List<itemObject>();
    List<itemObject> ingots = new List<itemObject>();
    List<itemObject> ores = new List<itemObject>();
    List<itemObject> misc = new List<itemObject>();
    List<itemObject> gas = new List<itemObject>();
    List<itemObject> power = new List<itemObject>();
    List<itemObject> ships = new List<itemObject>();

    //push each item into their respective list for rendering
    foreach(var thing in orderedTotals)
    {
        switch(thing.Type)
        {
            case "Component":
                components.Add(new itemObject(thing.Name, thing.Type, thing.Quantity));
                break;
            case "Ingot":
                ingots.Add(new itemObject(thing.Name, thing.Type, thing.Quantity));
                break;
            case "Ore":
                ores.Add(new itemObject(thing.Name, thing.Type, thing.Quantity));
                break;
            case "Gas":
                gas.Add(new itemObject(thing.Name, thing.Type, thing.Quantity));
                break;
            case "Power":
                power.Add(new itemObject(thing.Name, thing.Type, thing.Quantity));
                break;
            case "Ship":
                ships.Add(new itemObject(thing.Name, thing.Type, thing.Quantity));
                break;
            default:
                misc.Add(new itemObject(thing.Name, thing.Type, thing.Quantity));
                break;
        }
        

        //console.WriteText(thing.Name + " (" + thing.Type + ") " + thing.Quantity + "\n", true);
    }

    foreach(var thing in orderedPowerProducers)
    {
        power.Add(new itemObject(thing.Name, thing.Type, thing.Quantity));
    }

    //handle the display output
    console.WriteText(Display(components, ingots, ores, misc, gas, power, ships),true);
}

