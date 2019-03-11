using System;
using System.Collections.Generic;

namespace CSD{

public interface ITextDescriptionProvider{
	string GetDescription();
}

public interface ILocation{
	IEntity GetLocation();
	List<IEntity> GetContents();
}

}