using System;
using UnityEngine.SceneManagement;

public class ClassFlowchart : FlowChart
{
	public static Class rclass = Class.Count;

	private void Awake()
	{
		flowState = new FlowState("Basic", "You wake up inside of a cave, extremely dirty and dressed in rags. You’re terribly hungry and cannot remember a thing since before waking. Lifting yourself off the cave floor and heading toward the exit, you peer outside and to the left. Upon leaning out you hear footsteps to your right, pulling back in to try to hide yourself. You are almost positive whoever was traveling along the road must have seen you.", new FlowState[2]
		{
			new FlowState("Hide", "Basic", "You do not believe yourself fast enough to outrun the group and instead rely on how small you are to hide behind couple of boulders. To your dismay this did not help you in the slightest. “H’loo,” the voice of a woman shouts down the cave. “I saw you poke yer’ little head out just a moment ago, don’t hide from Nann!” The woman is friendly and has a gaggle of children following behind her. She takes you back with her to a nearby village to an orphanage to clean you up and question you. She asks if you’re a boy or a girl but you keep quiet until she strips you for cleaning.", new FlowState[3]
			{
				new FlowState("Boy", "Basic", "Years pass as you live with the other orphans and Naan, days go on as you all beg for change throughout the day to bring back to the orphanage to eat, play, and sleep at night. Eventually your days at the orphanage come near an end, a few more days and you will be old enough to begin working with a nearby fishmonger, traveling and trading. You stopped begging a long time ago due to being too old, and now mostly help around the house. Late in the day some of the newer children burst through the door all yelling at the same time. The only thing you manage to take from all of their voices overlapping is one word, raiders. You quickly close the door and run back to Naan’s room. She had become sick in her old age and rarely leaves her room, letting the children handle the house. You tell her of the raiders and ask what to do. “I’m too old to leave child,” she says to you while you nervously glance behind you. “I’m sure they won’t hurt no old lady huddled up in her death bed, go now and take the others with you!”.", new FlowState[2]
				{
					new FlowState("Leave the children", "Basic", "You decide that you are unable to take care of that many children, but don’t know how to get them to leave with the rest of the village. Instead of heading back to where the children are all gathered, you gather up your stash of food in your room and leave through one of the windows and around the back of the house, running up into the hills. Turning around, you see the children being ushered out the front of the house by another villager and along the road with the rest of the village following suit. You leave in the direction that you had been heading. Weeks later, you collapse from exhaustion nearby a large looking estate. There are sounds of hooves coming from behind you. Later you wake up inside of a room which is questionably familiar, an older man is next to your bed staring at you incredulously. “I can’t believe you finally came home,” the man says to you. You memories from before the orphanage begin to resurface, this man was your father and you were the successor to his estate. You had originally left home to inspire others with your music, which your father told you was not fit for the son of a knight and promptly destroyed your musical instrument.", new FlowState[2]
					{
						new FlowState("Stay", "Basic", "You give up your passions to once again live with your father and relieve him of the burdens of not having a successor to his estate. Time passes and more responsibilities are passed on to you. You are re-trained in combat as a knight, although a lot of your experience is soon remembered from when you were a child. Soon you are a young adult and your father passes away, truly leaving all that he has owned to you. One day in the morning you hear a commotion outside, some of the villagers of a nearby town are at your gate requesting your help. You see in the distance that their village is burning and a group of riders are quickly approaching your manse. You rush out of your room while dressing yourself in combat attire, all the while trying your hardest to reach the surviving villagers before the riders do. To your delight, you manage to reach the gate, properly adorned for combat with your personal sword with you. The villagers rush inside immediately upon you opening it. After closing and baring it, you lead the villagers back behind the house towards a hidden gate on the side of the walls. When the last surviving servant of your household escapes through the door, you rush back to the house, piling up as many flammable things as you possibly can. You light it on fire, making sure that the raiders can not use it as a functioning hideout before attempting to escape the way the villagers had. They cut you off at the back door, forcing the door closed and baring you inside with the fire you had started. Lucky for you, you know the house better than they do. You make your way to the front of the house, avoiding various fires that had spread and covering your mouth. Making your way through a window that is hidden by the stable, you sneak inside and take your horse. Before the raiders can react, you’re riding past them and out the destroyed front gate. None of them are mounted by the time you are out of sight so they do not pursue. Following the road, you begin your adventure as a knight without a home.", new FlowState[1]
						{
							new FlowState("Continue", Class.Heir)
						}),
						new FlowState("Leave", "Basic", "After remembering your passion to travel and inspire others with your words to whatever ends you had originally planned, you promptly left your father’s household to continue on your quest as a bard. I mean come on, the ass destroyed your instrument. Years pass and you become known widely for attending large gatherings as a singer, and joining certain military excursions as inspiration toward their goals.", new FlowState[1]
						{
							new FlowState("Continue", Class.Bard)
						})
					}),
					new FlowState("Take them with you", "Basic", "How quickly do you overcome self doubt?", new FlowState[2]
					{
						new FlowState("Slowly", Class.Page),
						new FlowState("Quicly", "Basic", "You come across a poor man, how do you help?", new FlowState[2]
						{
							new FlowState("Steal", Class.Rogue),
							new FlowState("Heal", Class.Sylph)
						})
					})
				}),
				new FlowState("Girl", "Basic", "For self or others?", new FlowState[2]
				{
					new FlowState("Self", "Basic", "Is it really kill or be killed?", new FlowState[2]
					{
						new FlowState("No", Class.Witch),
						new FlowState("Yes", "Basic", "Steal or destroy what you can't have?", new FlowState[2]
						{
							new FlowState("Steal", Class.Thief),
							new FlowState("Destroy", Class.Prince)
						})
					}),
					new FlowState("Others", "Basic", "Knowledge or Power?", new FlowState[2]
					{
						new FlowState("Knowledge", Class.Mage),
						new FlowState("Power", "Basic", "The leader or the people", new FlowState[2]
						{
							new FlowState("Leader", Class.Knight),
							new FlowState("People", Class.Maid)
						})
					})
				}),
				new FlowState("Stay quiet", "Basic", "For self or others?", new FlowState[2]
				{
					new FlowState("Self", "Basic", "Is it really kill or be killed?", new FlowState[2]
					{
						new FlowState("No", Class.Witch),
						new FlowState("Yes", "Basic", "Steal or destroy what you can't have?", new FlowState[2]
						{
							new FlowState("Steal", Class.Thief),
							new FlowState("Destroy", Class.Prince)
						})
					}),
					new FlowState("Others", "Basic", "Knowledge or Power?", new FlowState[2]
					{
						new FlowState("Knowledge", Class.Mage),
						new FlowState("Power", "Basic", "The leader or the people", new FlowState[2]
						{
							new FlowState("Leader", Class.Knight),
							new FlowState("People", Class.Maid)
						})
					})
				})
			}),
			new FlowState("Flee", "Basic", "Active/Passive", new FlowState[2]
			{
				new FlowState("Passive", "Basic", "For self or others?", new FlowState[2]
				{
					new FlowState("Self", "Basic", "Knowledge or power?", new FlowState[2]
					{
						new FlowState("Knowledge", Class.Seer),
						new FlowState("Power", "Basic", "How to escape the prison?", new FlowState[2]
						{
							new FlowState("Like an heir", Class.Heir),
							new FlowState("Like a bard", Class.Bard)
						})
					}),
					new FlowState("Others", "Basic", "How quickly do you overcome self doubt?", new FlowState[2]
					{
						new FlowState("Slowly", Class.Page),
						new FlowState("Quickly", Class.Rogue)
					})
				}),
				new FlowState("Active", "Basic", "For self or others?", new FlowState[2]
				{
					new FlowState("Self", "Basic", "Steal or destroy what you can't have?", new FlowState[2]
					{
						new FlowState("Steal", Class.Thief),
						new FlowState("Destroy", Class.Prince)
					}),
					new FlowState("Others", "Basic", "Knowledge or Power?", new FlowState[2]
					{
						new FlowState("Knowledge", Class.Mage),
						new FlowState("Leader", Class.Knight)
					})
				})
			})
		});
	}

	public override void TestFinished(Enum result)
	{
		rclass = (Class)(object)result;
		SceneManager.LoadScene("TestRoom");
	}
}
