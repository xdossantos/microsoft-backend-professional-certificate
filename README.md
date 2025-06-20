# TMWX Technical Interview

## The Problem
A busy car dealership has a list of customers waiting to test drive a vehicle. The waitlist is created sequentially (e.g., customers are added in a FIFO order) from the time they express interest. Once there is availability, the front desk calls each customer to offer the test drive in the order they were added to the waitlist. The sales associate at the front desk has noticed that they waste a lot of time trying to contact customers from the waitlist – they're often not available, don't pick up the phone, etc. They would like to generate a better list that will increase their chances of reaching a customer in the first few calls.
 
## The Brief
Using the customers’ demographics and behavioural data (see sample-data/customers.json), create an algorithm that will process a set of historical customer data and compute a score for each customer that represents the chance of a customer accepting the test drive offer from the waitlist (1 as the lowest, 10 as the highest). Take into consideration that customers who have little behavioural data should be randomly added to the top list so as to give them a chance to be selected. Expose an API that takes a facility's location as input and returns an ordered list of 10 customers who will most likely accept the appointment offer.
 
## Weighting Categories
Demographic
- age (weighted 10%)
- distance to practice (weighted 10%)
 
Behaviour
- number of accepted offers (weighted 30%)
- number of cancelled offers (weighted 30%)
- reply time (how long it took for patients to reply) (weighted 20%)
 
## Patient Model
- ID
- age (in years)
- location
 - lat
 - long
- acceptedOffers (integer)
- canceledOffers (integer)
- averageReplyTime (integer, in seconds)