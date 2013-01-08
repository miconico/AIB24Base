select * from budgettags
select * from budgetCredits

select * from budgetCredits order by 
select datediff(d,getdate(),cast('01/03/13' as DATE))
select cast(substring(DATE,4,2)+'/'+LEFT(date,2)+RIGHT(Date,3) as DATE) from budgetCredits

update budetTags set credit = (select SUM(credit) from budgetCredit where tag = budgetTags.tag group by tag),
		StartDate = (select min(cast(substring(DATE,4,2)+'/'+LEFT(date,2)+RIGHT(Date,3) as DATE)) where budgetTag = tag), EndDate = ()

select isnull((select SUM(credit) from budgetCredits where tag = budgetTags.tag group by tag),0) from budgetTags
use budget select * from budgetSpend
select min(cast(substring(DATE,4,2)+'/'+LEFT(date,2)+RIGHT(Date,3) as DATE)), MAX(cast(substring(DATE,4,2)+'/'+LEFT(date,2)+RIGHT(Date,3) as DATE)),tag,SUM(credit) 
from budgetCredits group by tag

update budgetTags set Start

Tag | Description | Account e.g. my salary account, hubbies bill account| Should this be included in all calculations | Group Tag e.g. Electricity Bill, Wages