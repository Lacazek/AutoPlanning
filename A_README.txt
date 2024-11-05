# Opti_Struct

Le but de ce code est de fournir une solution pour créer automatiquement une structure d'optimisation. 
Selon le fichier txt, l'utilisateur peut décider de créer sa propre structure.
Le script lit dans les fichiers txt, les opérations souhaitées par l'utilisateur. 
Il existe plusieurs fichiers en fonction de la localisation, latéralité et/ou de la prescription 
(disponible dans le dossier \File) (ils seront amenés à évoluer avec le temps).


****************************************************************************************************************************
****************************************************************************************************************************
****************************************************************************************************************************

Pour le bon déroulé du code, un vocabulaire spécifique doit être utilisé. Il respecte l'écriture suivante :

nom_structure_à_créer 		: 	nom_structure_1		 operateur	 nom_structure_2 	operateur 	nom_structure_3 	.....

Pour la table de traitement, la nomenclature est la même avec 4 possibilités :

table : fine 

table : moyenne

table : epaisse 

table : halcyon

Les ":" permettent de faire la différence entre le nom de la structre à créer (à gauche) des structures utilisée pour les opérations (à droite)

La partie operateur correspond aux opérations à effectuer. Il existe 6 opérateurs différents :


"|" pour la somme de deux structures

"&" pour récupérer uniquement l'intersection entre deux structures

"#" pour la soustraction entre deux structures

"^" pour récupérer uniquement l'union des structures

"!" pour créer une marge par rapport à une structure définie
    Pour l'utilisation de cet opérateur, la partie droite correspond à la marge en mm (exemple : PTV : ITV ! 5.00)

" " pour créer une structure vide

"," pour les marges asymétriques : droite, gauche, arrière, avant, bas puis haut


Le script génère automatiquement le contour externe en fonction de la loc (seuil à -700 UH pour le sein, -350 pour l'ORL sinon, par défaut -300 UH)

Le script génère automatiquement la table de traitement en fonction de la demande de l'utilisateur (via le fichier txt)

Une ligne contenant le mot verbose en premiere ligne de chaque fichier txt définit le niveau de verbosité; c-à-d le niveau de dialogue du code
Il existe deux niveaux :
	0 --> le code ne parle pas du tout (message de fin uniquement)
	1 --> le code parle et décrit chaque étape

Le code enregistre ce qui se passe lors de son exécution dans un fichier "LogFile.txt" peu importe le niveau de verbosité.
Le fichier LogFile est vidé lorsqu'il atteint 500 Ko pour éviter des erreurs de générations.

****************************************************************************************************************************
****************************************************************************************************************************
****************************************************************************************************************************

!!!! Le fichier txt ne doit pas être modifié durant l'exécution du script!!!


****************************************************************************************************************************
***************************************************** Exemple **************************************************************
****************************************************************************************************************************

// Marges symétriques


PTV : itv ! 5

ptv --> nom structure

itv --> structure 1

! --> opérateur pour les marges

5 --> 5.00 mm de marges isotropique [mm]

-------> Il créé la même structure avec 5 mm de marges


// Marges asymétriques


PTV : itv ! 1,2,3,4,5,6

ptv --> nom structure

itv --> structure 1

! --> opérateur pour les marges

5 --> 1.00 mm de marges à droite [mm]

5 --> 2.00 mm de marges à gauche [mm]

5 --> 3.00 mm de marges en arrière [mm]

5 --> 4.00 mm de marges en avant [mm]

5 --> 5.00 mm de marges en bas [mm]

5 --> 6.00 mm de marges en haut [mm]


-------> Il créé la même structure avec des marges asymétriques

// Création structure vide

PTV : 

PTV --> nom structure qui sera créée vide

-------> Il créé une structure vide


// Avec deux structures


PTV - ITV : ptv # itv

PTV - ITV --> nom de la structure à créer

ptv --> structure 1 pour l'opération

"#" --> c'est l'opérateur (soustraction)

itv --> structure 2 pour l'opération

-------> Il créé ici une structure correspondante à la couronne entre le PTV et l'ITV



// Avec n structures

PTV - ITV : ptv # itv # gtv

PTV - ITV --> nom de la structure à créer

ptv --> structure 1 pour l'opération

"#" --> c'est l'opérateur (soustraction)

itv --> structure 2 pour l'opération

"#" --> c'est l'opérateur (soustraction)

gtv --> structure 3 pour l'opération

!!!!! Ici, les opérations sont réalisés dans l'ordre de lecture.

-------> Il créé une structure correspondant à l'enchaînement des opérations


****************************************************************************************************************************
****************************************************************************************************************************
****************************************************************************************************************************

##### Prochaine MAJ pour améliorer la casse des noms (plus de problèmes de majuscule ou minuscule; Expression régulière ; ...) 		 terminé
##### Amélioration de la détection des mauvais noms de structures et assignation à des structures existente de manière manuel et/ou automatique 	terminé
##### Marges asymétrique  		terminé
##### Création auto table 		terminé
##### Création auto CE ? 		terminé
##### Amélioration du fichier log 	en cours ...
##### problème sur les parotides en ORL
##### Mauvaise prise en compte des latéralités (IMRT sein avec z_ptv sein et z_ptv sein G) 	terminé


