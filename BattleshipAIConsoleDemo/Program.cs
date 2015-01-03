using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace BattleshipAIConsoleDemo
{
	class Program
	{
		private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		static int MaximumMapSize = 5;
		static int[,] map = new int[MaximumMapSize, MaximumMapSize];

		static void Main(string[] args)
		{
			log4net.Config.XmlConfigurator.Configure();

			InitializeBoard();
			SeekShip(2);
		}

		private static void InitializeBoard()
		{
			for (int y = 0; y < MaximumMapSize; y++)
			{
				for (int x = 0; x < MaximumMapSize; x++)
				{
					map[x, y] = 0;
				}
			}
		}

		private static void SeekShip(int lengthOfShip)
		{
			// fire on best found positions until we run out of places to fire on

			List<ShipCoord> shipCells = FindShip(2);

			// log ship cells ordered descending by number of occurrences
			/*
			log.Debug("total shots left=" + NumberOfShotsLeft(shipCells));
			for (int i = 0; i < shipCells.Count; i++)
			{
				log.Debug(shipCells[i].X + "," + shipCells[i].Y + " [" + shipCells[i].Occurrences + "]");
			}
			*/
			while (NumberOfShotsLeft(shipCells) > 0)
			{
				// take a shot
				map[shipCells[0].X, shipCells[0].Y] = 1;
				log.Debug(shipCells[0].X + "," + shipCells[0].Y);

				shipCells = FindShip(2);
				/*
				log.Debug("total shots left=" + NumberOfShotsLeft(shipCells));
				for (int i = 0; i < shipCells.Count; i++)
				{
					log.Debug(shipCells[i].X + "," + shipCells[i].Y + " [" + shipCells[i].Occurrences + "]");
				}
				*/
			}
		}

		private static int NumberOfShotsLeft(List<ShipCoord> shipCells)
		{
			int total = 0;

			for (int i = 0; i < shipCells.Count; i++)
			{
				if (shipCells[i].Occurrences > 0)
				{
					total++;
				}
			}

			return total;
		}

		private static List<ShipCoord> FindShip(int lengthOfShip)
		{
			// find all the places where a 1x3 ship could be positioned
			List<ShipPosition> possibleShipLocations = FindAllShipLocations(lengthOfShip);
			/*
			for (int i = 0; i < possibleShipLocations.Count; i++)
			{
				log.Debug("Possible Ship Location:" + possibleShipLocations[i].X + "," + possibleShipLocations[i].Y+" ("+(possibleShipLocations[i].VerticalOrientation ? "Vertical" : "Horizontal")+")");
			}
			*/
			List<ShipCoord> allCoordinates = new List<ShipCoord>();

			// dump all the ship coordinates
			for (int i = 0; i < possibleShipLocations.Count; i++)
			{
				/*
				if (possibleShipLocations[i].VerticalOrientation)
				{
					log.Debug("Vertical");
				}
				else
				{
					log.Debug("Horizontal");
				}
				*/
				for (int j = 0; j < lengthOfShip; j++)
				{
					if (possibleShipLocations[i].VerticalOrientation)
					{
						allCoordinates.Add(new ShipCoord
						{
							X = possibleShipLocations[i].X,
							Y = possibleShipLocations[i].Y + j
						});

						//log.Debug(possibleShipLocations[i].X + "," + (possibleShipLocations[i].Y + j));
					}
					else
					{
						allCoordinates.Add(new ShipCoord
						{
							X = possibleShipLocations[i].X + j,
							Y = possibleShipLocations[i].Y
						});
						//log.Debug((possibleShipLocations[i].X + j) + "," + possibleShipLocations[i].Y);
					}
				}
			}

			if (allCoordinates.Count == 0)
			{
				return allCoordinates;
			}

			// sort list by x then y
			var sortedList = (from a in allCoordinates select a).OrderBy(x => x.X).ThenBy(y => y.Y).ToList();

			// dump the sorted list
			/*
			for (int i = 0; i < sortedList.Count; i++)
			{
				log.Debug(sortedList[i].X + "," + sortedList[i].Y);
			}

			log.Debug("");
			*/

			// build list of coords, by count of occurrences
			List<ShipCoord> countedList = new List<ShipCoord>();

			int prevx = sortedList[0].X;
			int prevy = sortedList[0].Y;
			int total = 0;
			for (int i = 0; i < sortedList.Count; i++)
			{
				if (sortedList[i].X != prevx || sortedList[i].Y != prevy)
				{
					countedList.Add(new ShipCoord
					{
						X = prevx,
						Y = prevy,
						Occurrences = total
					});

					//log.Debug(prevx + "," + prevy + " [" + total + "]");
					total = 1;
					prevx = sortedList[i].X;
					prevy = sortedList[i].Y;
				}
				else
				{
					total++;
				}
			}

			// need to account for the last one
			//log.Debug(prevx + "," + prevy + " [" + total + "]");

			countedList.Add(new ShipCoord
			{
				X = prevx,
				Y = prevy,
				Occurrences = total
			});

			// sort by occurrences
			return countedList.OrderByDescending(c => c.Occurrences).ToList();
		}

		// find all the possible vertical and horizontal ship positions available on the map with a ship length of lengthOfShip.
		private static List<ShipPosition> FindAllShipLocations(int lengthOfShip)
		{
			List<ShipPosition> result = new List<ShipPosition>();

			for (int y = 0; y < MaximumMapSize; y++)
			{
				for (int x = 0; x < MaximumMapSize; x++)
				{
					bool possiblePosition = true;

					// do all the verticle ships first.
					for (int i = 0; i < lengthOfShip; i++)
					{
						if (y + i >= MaximumMapSize)
						{
							possiblePosition = false;
							break;
						}

						if (map[x, y + i] != 0)
						{
							possiblePosition = false;
							break;
						}
					}

					if (possiblePosition)
					{
						ShipPosition shipPosition = new ShipPosition
						{
							X = x,
							Y = y,
							VerticalOrientation = true
						};

						result.Add(shipPosition);
					}

					possiblePosition = true;

					// do all the horizontal ships
					for (int i = 0; i < lengthOfShip; i++)
					{
						if (x + i >= MaximumMapSize)
						{
							possiblePosition = false;
							break;
						}

						if (map[x + i, y] != 0)
						{
							possiblePosition = false;
							break;
						}
					}

					if (possiblePosition)
					{
						ShipPosition shipPosition = new ShipPosition
						{
							X = x,
							Y = y,
							VerticalOrientation = false
						};

						result.Add(shipPosition);
					}
				}
			}

			return result;
		}
	}
}
