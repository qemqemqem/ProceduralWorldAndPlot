
using System.Collections.Generic;

namespace CSD{

	public class Situation : EventComponent {
		//fit the situation to the scene
		//assign things to roles within the context of the situation
		//establish parameterized objectives to suggest to the agents in certain roles based on their beliefs about the situation
		//have the situation track roles and check for entering new situations when objectives are achieved and the situation ends

	}

	public interface AIInterface {
		EventComponent GetBestAction (Objective objective);
		List<Objective> GetSortedObjectives (Objective objective);
	}
}
















































//Conflicts
//will the party avoid detection by Zarkon's forces or will they be forced into a bloody confrontation
//will you find the missing person alive or will it be too late
//will you find someone that knows what the rune means or will it forever stay a mystery
//will the the guard take chase and leave his post
//will you the gladiators turn on each other so you have fewer opponents to face
//will the trolls see past your trick or bicker until the sun turns them to stone
//will the ship be completed before the meteor arrives and there is no time to evacuate
//will the village have enough gold to tithe the dragon
//will the village have enough food to survive the winter
//will the robot rescue the injured human before the other robots kill her companion
//will the murderbot escape the station before it falls into the planet
//will the travelers survive the mountain pass or will they be rebuffed by it's challenges
//will the adventurer turn the urchin over to the guards or will they stand up to defend the weak
//will the once merciless now reformed soldier revert to his evil ways or will he stand strong against temptation
//will the star crossed lovers reunite before it's too late
//will young girl pay of her parents debt and rescue them before they are turned into dinner
//will the forest spirits and humans fight to the death or will the distant traveler bring peace to the land
//will grant see that his true love has been in front of him the whole time and stop trying to impress his ex

/* Fights
		 * Chases
		 * Thefts
		 * Insults
		 * Races
		 * Bets
		 * Battle Royales
		 * Sports
		 * 
		 */





//this is a digraph of objecties that trigger new events


//crime/offense happens - assault, vandilism, theft, insults, 
//observe
//difuse
//instigate
//use distraction to do something else
//chase
//observe
//interviene to help pursuer or pursued
//hide/reveal, trip/block, capture
//use distraction to do something else
//capture
//take and release custody to self/other
//rob
//beat
//kill
//fight
//observe
//interviene to help one party or the other
//use distraction to do something else
//making/prepairing something
//building something large
//having a meal
//a celebration
//a game or context
//haggling/making a purchase