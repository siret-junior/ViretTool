#install.packages("rjson")

filterQueries = function(taskBoundariesMatrix, actions, savedQueries) {
  
  resultQueries = c()
  taskId = 1
  
  closestResetAll = suppressWarnings(max(as.numeric(t(t(actions[,actions[2,] == "ResetAll" & actions[1,] < taskBoundariesMatrix[1,taskId]]))[1,])))
  resultActions = actions[,actions[1,] >= closestResetAll]
  
  resultTaskBoundaries = list("Closes_ResetAll" = closestResetAll, "Server_Start" = taskBoundariesMatrix[1,taskId],"Server_End" = taskBoundariesMatrix[2,taskId])
  
  for (queryId in 1:length(savedQueries))
  {
    if (savedQueries[queryId] > taskBoundariesMatrix[2,taskId] && taskId < length(taskBoundariesMatrix[1,])) { #after end
      taskId = taskId + 1
      closestResetAll = suppressWarnings(max(as.numeric(t(t(resultActions[,resultActions[2,] == "ResetAll" & resultActions[1,] < taskBoundariesMatrix[1,taskId]]))[1,])))
      resultActions = resultActions[,resultActions[1,] <= taskBoundariesMatrix[2,taskId-1] | resultActions[1,] >= closestResetAll]
      resultTaskBoundaries = rbind(resultTaskBoundaries, list(closestResetAll,taskBoundariesMatrix[1,taskId],taskBoundariesMatrix[2,taskId]))
    }
    
    if (savedQueries[queryId] >= taskBoundariesMatrix[1,taskId] || (!is.infinite(closestResetAll) && savedQueries[queryId] > closestResetAll)) { #earlier than start
      if (savedQueries[queryId] <= taskBoundariesMatrix[2,taskId]) 
      {
        resultQueries = append(resultQueries, savedQueries[queryId])  
      }
    } 
  }
  
  return(list(resultQueries, resultActions, resultTaskBoundaries))
}



library("rjson")

baseDir = "d:\\Temp\\Downloads\\database-vbs2019\\"

#read valid tasks
competitionsFile = file(paste0(baseDir, "competitions.db"),open="r")
validTasks = fromJSON(readLines(competitionsFile)[2])[["taskSequence"]]
close(competitionsFile)

taskFile = file(paste0(baseDir, "tasks.db"),open="r")
taskLines = readLines(taskFile)
close(taskFile)
validTasks = validTasks[validTasks %in% sapply(taskLines, function(line) {
  jsonData = fromJSON(line)
  ifelse(is.element(jsonData[["_id"]], validTasks) && !startsWith(jsonData[["type"]], "AVS"), jsonData[["_id"]], "")
})]

actionsFile = file(paste0(baseDir, "actionLogs.db"),open="r")
actionLines = readLines(actionsFile)
close(actionsFile)

submissionsFile = file(paste0(baseDir, "submissions.db"),open="r")
submissionsLines = readLines(submissionsFile)
close(submissionsFile)


#tasks definitions
taskBoundariesMatrix = c()
for (line in taskLines)
{
  jsonData = fromJSON(line)
  if (!is.element(jsonData[["_id"]], validTasks)) {
    next
  }
  taskBoundariesMatrix = cbind(taskBoundariesMatrix, c(jsonData[["startTimeStamp"]], jsonData[["endTimeStamp"]]))
}

taskBoundariesMatrix = taskBoundariesMatrix[,order(taskBoundariesMatrix[1,])]
taskBoundaries = as.vector(taskBoundariesMatrix)

for (member in c(0,1)) {
  
  #actions
  actions = c()
  for (line in actionLines)
  {
    actionData = fromJSON(line)
    if (is.null(actionData[["teamId"]]) || actionData[["teamId"]] != 4 || actionData[["memberId"]] != member
        || !is.element(actionData[["taskId"]], validTasks)
    ) {
      next
    }
    
    actions = cbind(actions, sapply(actionData[["events"]], 
                                    function(event) { c(event[["timestamp"]],event[["type"]],event[["category"]],
                                                        ifelse(is.null(event[["value"]]), "NA", event[["value"]])) }))
  }
  
  #submissions
  submissions = c()
  for (line in submissionsLines)
  {
    subData = fromJSON(line)
    if (is.null(subData[["teamNumber"]]) || subData[["teamNumber"]] != 4 || subData[["memberNumber"]] != member
        || !is.element(subData[["taskId"]], validTasks)
    ) {
      next
    }
    
    submissions = cbind(submissions, c(subData[["timestamp"]], ifelse(is.null(subData[["correct"]]), 0, subData[["correct"]])))
  }
  
  #saved tasks
  baseSavedQPath = "..\\..\\..\\Logs\\2019-01-09_VBS2019\\"
  savedQueries = as.numeric(sub(".json$", "", list.files(paste0(baseSavedQPath, ifelse(member == 0,"SIRIUS-PC","PREMEK-NTB"), "\\QueriesLog"))))
  newQueriesAndActions = filterQueries(taskBoundariesMatrix, actions, savedQueries)
  savedQueries = newQueriesAndActions[[1]]
  actions = newQueriesAndActions[[2]]
  
  #actions[1,] = as.numeric(actions[1,])
  write.table(t(submissions), paste0("Submissions_member_",member,".csv"), col.names = FALSE, row.names = FALSE, sep = ";")
  write.table(t(actions[,actions[3,] == "Browsing"]), paste0("Actions_member_",member,".csv"), col.names = FALSE, row.names = FALSE, sep = ";")
  write.table(newQueriesAndActions[[3]], paste0("TaskBoundaries_member_",member,".csv"), row.names = FALSE, sep = ";")
  
  #transform queries
  transformedQueries = c()
  for (query in savedQueries) {
    queryFile = file(paste0(baseSavedQPath,ifelse(member == 0,"SIRIUS-PC","PREMEK-NTB"),"\\QueriesLog\\",query,".json"),open="r")
    queryData = fromJSON(file = queryFile)
    close(queryFile)
    
    first = ifelse(queryData[["primarytemporalquery"]] == 0, "formerquery", "latterquery")
    second = ifelse(queryData[["primarytemporalquery"]] == 1, "formerquery", "latterquery")
    similarity = queryData[["bitemporalsimilarityquery"]]
    
    #TODO real key words?
    transformedQuery = list(
      "TimeStamp" = query,
      "KW_1_Count" = length(similarity[["keywordquery"]][[first]][["synsetgroups"]]),
      "KW_2_Count" = length(similarity[["keywordquery"]][[second]][["synsetgroups"]]),
      "Color_1" = ifelse(length(similarity[["colorsketchquery"]][[first]][["colorsketchellipses"]]) > 0, "on", "off"),
      "Color_2" = ifelse(length(similarity[["colorsketchquery"]][[second]][["colorsketchellipses"]]) > 0, "on", "off"),
      "Face_1" = ifelse(length(similarity[["facesketchquery"]][[first]][["colorsketchellipses"]]) > 0, "on", "off"),
      "Face_2" = ifelse(length(similarity[["facesketchquery"]][[second]][["colorsketchellipses"]]) > 0, "on", "off"),
      "Text_1" = ifelse(length(similarity[["textsketchquery"]][[first]][["colorsketchellipses"]]) > 0, "on", "off"),
      "Text_2" = ifelse(length(similarity[["textsketchquery"]][[second]][["colorsketchellipses"]]) > 0, "on", "off"),
      "Semantic_1" = ifelse(length(similarity[["semanticexamplequery"]][[first]][["positiveexampleids"]]) > 0, "on",
                            ifelse(length(similarity[["semanticexamplequery"]][[first]][["externalimages"]]) > 0, "external", "off")),
      "Semantic_2" = ifelse(length(similarity[["semanticexamplequery"]][[second]][["positiveexampleids"]]) > 0, "on", 
                            ifelse(length(similarity[["semanticexamplequery"]][[second]][["externalimages"]]) > 0, "external", "off")),
      "SortedBy" = switch(as.numeric(queryData[[ifelse(queryData[["primarytemporalquery"]] == 0, "formerfusionquery", "latterfusionquery")]][["sortingsimilaritymodel"]]) + 1,
                          "Keyword", "ColorSketch", "Face", "Text", "Semantic", "None")
    )
    
    transformedQueries = rbind(transformedQueries, transformedQuery)
  }
  write.table(transformedQueries, paste0("Queries_member_",member,".csv"), row.names = FALSE, sep = ";")
}





