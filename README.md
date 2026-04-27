# VoidBeat
SideScroller Game

Priorités de Développement - VoidBeat
Ce document suit l'évolution du projet, du prototype technique à la version finale.

🟢 Phase Alpha :
Objectif : Prototype minimaliste opérationnel.

Objectif 1 : Déplacements de Base et Parkour

[x] Implémenter la mécanique de course automatique du personnage vers la droite.

[x] Programmer la gestion de la verticalité, incluant la fonctionnalité de saut.

[x] Intégrer la mécanique de glissade.

[x] Développer le système de "Dash Multidirectionnel" pour permettre des trajectoires non linéaires à haute vitesse.

[x] Programmer le "Coyote Time" (marge d'erreur pour les actions).

Objectif 2 : Implémentation du Rythme et du Flow

[x] Intégrer un système de tempo basique.

[x] Créer l'interface de la barre de "Boost Cinétique".

[x] Programmer le système où chaque action réussie en rythme (saut, attaque) alimente cette barre de boost.

Objectif 3 : La Menace du Trou Noir (Condition de Défaite)

[x] Représenter visuellement la menace du trou noir sur le bord gauche de l'écran.

[x] Programmer la mécanique d'approche du trou noir si le rythme du joueur chute.

[x] Implémenter la condition de défaite : le joueur est aspiré si le trou noir se rapproche trop.

Objectif 4 : Interactions, Obstacles et Combat

[x] Placer des ennemis de test, servant de "piles d'endurance", sur le parcours.

[x] Implémenter le système de combat rythmique : réaliser une action au bon moment redonne instantanément du dash et de l'endurance.

[x] Ajouter des débris ou obstacles basiques nécessitant le maintien d'une glissade pour être franchis.

Objectif 5 : Critères de Validation du Niveau Test

[x] Ajout de checkpoints.

[x] Configurer une fin de niveau pour valider la condition de victoire.

🟡 Phase Bêta :
Objectif : Implémenter les mécaniques avancées, le pacing et ajouter les feedback (VFX/SFX).

Objectif 1 : Gestion du Flow & Combat

[x] T2.1 - Programmer la jauge de "Boost Cinétique" alimentée par les actions réussies.

[x] T2.2 - Lier dynamiquement la distance entre le joueur et le trou noir au niveau de la jauge.

[x] T2.3 - Implémenter le Dash Multidirectionnel.

[ ] T2.5 - Création splash screen, menu écran titre, menu principal, crédits, popup quitter, options vidéo audio calibration et commandes, menu pause et écran de victoire, menu sélection niveaux

[ ] T2.4 - Implémenter le système de pénalité de désynchronisation (perte d'endurance si action "hors-tempo").

Objectif 2 : Dynamisme, Environnement & Pacing

[x] T2.5 - Développer le système de BPM dynamique (accélération fluide de la piste audio et de la vitesse de jeu).

[x] T2.6 - Implémenter le modèle de difficulté en "Dents de scie" (Chute de BPM post-checkpoint et transitions ralenties).

[x] T2.7 - Programmer les "Couloirs Narratifs" (zones sans ennemis, caméra surélevée, filtre audio low-pass).


Objectif 3 : Feedback Sensoriel (Juice)

[ ] T2.8 - Première passe VFX/SFX : Screen shake, flashs néon synchronisés et retours sonores d'impact.

[ ] T2.9 - Amélioration visuelle du noyau K-Z0 (clignotement orange/rouge selon le seuil critique d'endurance).

[ ] T2.10 - Ajouter le feedback visuel (écharpe énergétique/traînée) pour souligner les trajectoires de saut et de dash.

Objectif 4 : Admin debug

[ ] - Touches de debug admin, téléportation entre les checkpoints / niveaux, invicibilité.

🔴 Phase Release : Finition & Narration
Objectif : Peaufiner afin de passer d'un prototype à un jeu complet.

Objectif 1 : Antagoniste & Boss Final (Le Cœur)

[ ] T3.1 - Développer l'IA de Néant-X (Phase 1) : Saut/Glissade esquivant les ondes de choc et piliers de code.

[ ] T3.2 - Développer l'IA de Néant-X (Phase 2) : Parkour aérien (Dash) entre les débris et vides gravitationnels.

[ ] T3.3 - Développer l'IA de Néant-X (Phase 3) : Attaques synchronisées sur le boss à 150+ BPM.

[ ] T3.4 - Programmer la phase de transition narrative (dialogue de l'IA) précédant le combat final.

Objectif 2 : Intégration Narrative & UI UX

[ ] T3.5 - Créer les Menus Principaux (Accueil, Crédits, Options : Vidéo/Audio/Commandes/Calibration de latence).

[ ] T3.6 - Implémenter l'écran de "Sélection des Niveaux" (Global Map) et le Menu Pause in-game (Sidebar).

[ ] T3.7 - Mettre en place le système de déclenchement des cinématiques in-game (Début, Fin de jeu).

[ ] T3.8 - Créer l'interface utilisateur (HUD) diégétique (Score dynamique glitché affiché in-world).

[ ] T3.9 - Implémenter le système de progression (déblocage de zones, gestion des fragments collectés, sauvegarde).

Objectif 3 : Optimisation & Équilibrage

[ ] T3.10 - Optimisation technique : Shaders de distorsion gravitationnelle, effet de spaghettification pour garantir un framerate constant.

[ ] T3.11 - Équilibrage final : Ajustement précis des fenêtres de tolérance rythmique (ms) et lissage de la courbe de difficulté.

🖌 Assets Graphique :
Main character / K-Z0 :

Spritesheet mouvements :

[ ] - Course

[ ] - Saut

[ ] - Glissade

[ ] - Chute

[ ] - Dash

[ ] - Attaque

[ ] - Dégâts subi

[ ] - Idle

Character Diegetic UI :

[ ] - Noyau (avec états colorimétriques)

[ ] - Jauge

[ ] - Écharpe énergétique (particules)

Ennemies :

[ ] - Drone (Pile d'endurance)

[ ] - Sentinelle (Bouclier magenta synchronisé)

[ ] - Néant X (Œil cyclopéen/Noyau géométrique)

Environnements (Tilemaps) :

[ ] - Intérieur du bunker (Béton, industriel)

[ ] - Mégalopole / Ville (Néons, gratte-ciels)

[ ] - Horizon du trou noir (Violet/Noir, distorsion)

[ ] - Coeur du trou noir (Espace blanc, numérique, fractales)

[ ] - Écran de sélection des niveaux (Carte fragmentée)

Particules et VFX :

[ ] - Onde gravitationnelle

[ ] - Particules void

[ ] - Effets glitch et aberration chromatique

[ ] - Impact attaque (Cyan électrique)

[ ] - Feedbacks d'interaction (Néon vert pour la validation)

Shaders :

[ ] - Effet spaghetti (étirement des pixels bord gauche)

[ ] - Distorsion gravitationnelle (effet lentille)
