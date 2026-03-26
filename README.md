# VoidBeat

SideScroller Game

Priorités de Développement - VoidBeat

Ce document suit l'évolution du projet, du prototype technique à la version finale.

<img width="945" height="592" alt="image" src="https://github.com/user-attachments/assets/a37b94b7-bce8-43e2-8723-dc58bd2cf1fb" />

🟢 Phase Alpha :

Objectif : Prototype minimaliste opérationnel.

* Objectif 1 : Déplacements de Base et Parkour
* [x] Implémenter la mécanique de course automatique du personnage vers la droite.

* [x] Programmer la gestion de la verticalité, incluant la fonctionnalité de saut.

* [x] Intégrer la mécanique de glissade.

* [x] Développer le système de "Dash Multidirectionnel" pour permettre des trajectoires non linéaires à haute vitesse.


* Objectif 2 : Implémentation du Rythme et du Flow
* [x] Intégrer un système de tempo basique.

* [x] Créer l'interface de la barre de "Boost Cinétique".

* [x] Programmer le système où chaque action réussie en rythme (saut, attaque) alimente cette barre de boost.

* Objectif 3 : La Menace du Trou Noir (Condition de Défaite)
* [x] Représenter visuellement la menace du trou noir sur le bord gauche de l'écran.

* [x] Programmer la mécanique d'approche du trou noir si le rythme du joueur chute.

* [x] Implémenter la condition de défaite : le joueur est aspiré si le trou noir se rapproche trop.

* Objectif 4 : Interactions, Obstacles et Combat
* [x] Placer des ennemis de test, servant de "piles d'endurance", sur le parcours.

* [x] Implémenter le système de combat rythmique : réaliser une action au bon moment redonne instantanément du dash et de l'endurance.

* [ ] Ajouter des débris ou obstacles basiques nécessitant le maintien d'une glissade pour être franchis.

* Objectif 5 : Critères de Validation du Niveau Test
* [ ] Configurer une fin de niveau pour valider la condition de victoire : survivre jusqu'au bout du parcours.

* [ ] S'assurer que l'exécution rythmique d'une attaque offre bien un retour visuel, et un boost de vitesse.

* [ ] Valider que le joueur ressent la pression temporelle constante induite par le bord gauche de l'écran.

---

🟡 Phase Bêta :

Objectif : Implémenter les mécaniques avancées et ajouter les feedback (VFX/SFX).

- Objectif 1 : Gestion du Flow & Combat

 * [ ] T2.1 - Programmer la jauge de "Boost Cinétique" alimentée par les actions réussies.
 * [ ] T2.2 - Lier dynamiquement la distance entre le joueur et le trou noir au niveau de la jauge.
 * [ ] T2.3 - Implémenter le Dash Multidirectionnel avec système de Target Lock sur les ennemis aériens.
 * [ ] T2.4 - Créer les "Ennemis-Notes" : Obstacles destructibles servant de déclencheurs rythmiques.

- Objectif 2 : Dynamisme & Environnement

 * [ ] T2.5 - Développer le système de BPM dynamique (accélération fluide de la piste audio et de la vitesse de jeu).
 * [ ] T2.6 - Implémenter les mécaniques de gravité changeante (inversion et étirement spatial).

- Objectif 3 : Feedback Sensoriel (Juice)

 * [ ] T2.7 - Première passe VFX/SFX : Screen shake, flashs néon synchronisés et retours sonores d'impact.

🔴 Phase Release : Finition & Narration

Objectif : Pauffiner afin de passer d'un prototype à un jeu complet.

- Objectif 1 : Antagoniste & Boss Final

 * [ ] T3.1 - Développer l'IA de Néant-X : Patterns d'attaque rythmiques, vagues de débris et ondes de choc.

- Objectif 2 : Intégration Narrative & UI

 * [ ] T3.2 - Mettre en place le système de déclenchement des journaux audio et des cinématiques in-game.
 * [ ] T3.3 - Créer l'interface utilisateur (HUD) diégétique, les menus et le système de sauvegarde.
 * [ ] T3.4 - Implémenter le tableau des scores et les multiplicateurs.

- Objectif 3 : Optimisation & Équilibrage

 * [ ] T3.5 - Optimisation technique : Shaders de distorsion gravitationnelle et scripts pour garantir un framerate constant.
 * [ ] T3.6 - Équilibrage final : Ajustement précis des fenêtres de tolérance (ms) et de la courbe de difficulté.

🖌 Assets Graphique :

- Main character / K-Z0 :

  - Spritesheet mouvements :

  * [ ] - Course
  * [ ] - Saut
  * [ ] - Glissade
  * [ ] - Chute
  * [ ] - Dash
  * [ ] - Attaque
  * [ ] - Dégâts subi
  * [ ] - Idle

  - Character Diegetic UI :

  * [ ] - Noyau
  * [ ] - Jauge 
  * [ ] - Echarpe

- Ennemies :

  * [ ] - Drone
  * [ ] - Sentinelle
  * [ ] - Néant X

- Environnements :

  - Tilemaps :

  * [ ] - Intérieur du bunker
  * [ ] - Mégapole / Ville
  * [ ] - Horizon du trou noir
  * [ ] - Coeur du trou noir

- Particules et VFX :

  * [ ] - Onde gravitationelle
  * [ ] - Particules void
  * [ ] - Effets glitch et aberration chromatique
  * [ ] - Impacte attaque

- Shaders

  * [ ] - Effet spaghetti
  * [ ] - Distorsion gravitationelle
