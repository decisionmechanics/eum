# Expected Utility Model

(Lack of) reproducibility in science is a big deal. Reviews of papers in the fields of psychology and cancer biology found that only 40% and 10%, respectively, of the results, could be reproduced.

Nature published the [results of a survey](http://www.nature.com/news/1-500-scientists-lift-the-lid-on-reproducibility-1.19970) of researchers in 2016 that reported

- 52% of researchers think there is a significant reproducibility crisis
- 70% of scientists have tried but failed to reproduce another scientist’s experiments

One of the joys of computer modelling research is that reproducibility is trivial. Even when there’s a random element we can set a seed. Producing code to formalise ideas makes it easier to experiment with and extend research.

Bruce Bueno de Mesquita has [published extensively](https://en.wikipedia.org/wiki/Bruce_Bueno_de_Mesquita) on a game theory-based predictive model. It has been used to perform [more than a thousand predictions for the CIA](http://www.nytimes.com/2009/08/16/magazine/16Bruce-t.html). However, despite many (refereed) papers and books on the model it’s never been *fully* specified.

A New York Times article on Bueno de Mesquita’s approach said that

> …Bueno de Mesquita does not publish the actual computer code of his model. (Bueno de Mesquita cannot do so because his former firm owns the actual code, but he counters that he has outlined the math behind his model in enough academic papers and books for anyone to replicate something close to his work.)

In an age where there is growing concern about the [impact of society on the use of “black box” decision-making models](https://www.amazon.com/Weapons-Math-Destruction-Increases-Inequality/dp/0553418815), it’s worth trying to shed light on ideas that could be useful in public policy work.

Attempts have been made to replicate Bueno de Mesquita’s work. Notably, [Schloz *et al*](http://journals.sagepub.com/doi/abs/10.1177/0951629811418142?legid=spjtp%3B23%2F4%2F510&patientinform-links=yes&) tried to complete the equations and processes used by Bueno de Mesquita. This paper provided a lot of clarification. Unfortunately, no code was published with the paper—so replication of the *replication* is non-trivial.

The code provided here is an attempt to *formally* replicate Bueno de Mesquita’s model using Scholz *et al*’s interpretation as a basis. It has been design to map closely to the mathematical formulations used in the literature—making it easier to compare the mathematical formulations and the code. This has been the overriding design criterion—as opposed to performance or engineering concerns.

While the code has largely followed the formulation of Scholz *et al* it has not been possible to get close to replicating the results presented in that paper—illustrating the value of the formality that code brings to research.

An independent attempt to replicate Scholz *et al*’s work has been undertaken—with [code](https://github.com/jmckib/bdm-scholz-expected-utility-model) provided. This work *has* managed to replicate the results in the paper.

However, to replicate the results, the author had to make some assumptions (regarding calculation of the probability of a successful challenge) that deviate from the formulations given in the literature. As the author notes, these assumptions don’t seem to have particular merit from a theoretical perspective. From comments in the code it appears that the author had access to the code used by Scholz *et al* as the published code clearly departs from the equations in the literature.

The comments in the code are worth reading as the developer has some interesting things to say about the model.

It appears that, despite attempts to formalise his process, Bueno de Mesquita’s approach remains a mystery.

The code published here attempts to stay true to the equations in Scholz *et al* as that seems to be a more valuable starting point for subsequent research. It is hoped that this may assist further research into an approach that, while attracting some [controversy](http://decision-making.moshe-online.com/bueno_de_mesquita_iran.html),  has also attracted significant interest and has been used extensively by at least one government agency.
