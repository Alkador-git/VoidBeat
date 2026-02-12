# VoidBeat
SideScroller Game

Priorités de Développement - VoidBeat

Ce document suit l'évolution du projet, du prototype technique à la version finale.

🟢 Phase Alpha :

Objectif : Valider la boucle de gameplay fondamentale (Input + Rythme). Utilisation de placeholders géométriques.

- Objectif 1 : Système de Rythme & Conductor

 * [ ] T1.1 - Créer le "Conductor" : Système de tracking du temps en millisecondes (ms) indépendant du framerate.
 * [ ] T1.2 - Implémenter le système de détection de "Hit Windows" (Perfect, Good, Miss) basé sur le BPM.
 * [ ] T1.3 - Mettre en place le système de synchronisation audio/visuel pour les notes.

- Objectif 2 : Mouvement & Physique de base

 * [ ] T1.4 - Développer le moteur de mouvement : Course automatique stable et physique du saut/glissade.
 * [ ] T1.5 - Programmer "L'Horizon de la Mort" : Ligne de collision gauche qui déclenche le Game Over si rattrapée.

- Objectif 3 : Architecture de Niveau

 * [ ] T1.6 - Créer un système de spawning d'obstacles (cubes/sphères) synchronisé sur la timeline musicale.

🟡 Phase Bêta :

Objectif : Implémenter les mécaniques avancées et injecter le "Feeling" (VFX/SFX).

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

🔴 Phase V1 : Finition & Narration

Objectif : Transformer le prototype en une expérience viscérale, narrative et optimisée.

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

  - Drones, sentinelles, boss / néant X :

  * [ ] - Drone
  * [ ] - Sentinelle
  * [ ] - Néant X

- Environnements :

  - Tilemaps :

  * [ ] - Intérieur du bunker
  * [ ] - Mégapole / Ville
  * [ ] - Horizon du trou noir
  * [ ] - Coeur du trou noir

  - Decorations :

  * [ ] - 
  * [ ] - 
  * [ ] - 
  * [ ] -

- Particules et VFX :

  * [ ] - Onde gravitationelle
  * [ ] - Particules void
  * [ ] - Effets glitch et aberration chromatique
  * [ ] - Impacte attaque

- Shaders

  * [ ] - Effet spaghetti
