#install.packages("rjson")

filterQueries = function(taskBoundariesMatrix, actions, savedQueries) {
  
  resultQueries = c()
  taskId = 1
  
  closestResetAll = max(as.numeric(taskBoundariesMatrix[1,taskId])- 20000,
                        suppressWarnings(max(as.numeric(t(t(actions[,actions[2,] == "ResetAll" & actions[1,] < taskBoundariesMatrix[1,taskId]]))[1,]))))
  resultActions = actions[,actions[1,] >= closestResetAll]
  
  resultTaskBoundaries = list("Closes_ResetAll" = closestResetAll, "Server_Start" = taskBoundariesMatrix[1,taskId],"Server_End" = taskBoundariesMatrix[2,taskId],
                              "TaskName" = taskBoundariesMatrix[3,taskId], "TaskId" = taskBoundariesMatrix[4,taskId])
  
  for (queryId in 1:length(savedQueries))
  {
    if (savedQueries[queryId] > taskBoundariesMatrix[2,taskId] && taskId < length(taskBoundariesMatrix[1,])) { #after end
      taskId = taskId + 1
      closestResetAll = max(as.numeric(taskBoundariesMatrix[1,taskId]) - 20000,
                            suppressWarnings(max(as.numeric(t(t(actions[,actions[2,] == "ResetAll" & actions[1,] < taskBoundariesMatrix[1,taskId]]))[1,]))))
      resultActions = resultActions[,resultActions[1,] <= taskBoundariesMatrix[2,taskId-1] | resultActions[1,] >= closestResetAll]
      resultTaskBoundaries = rbind(resultTaskBoundaries, list(closestResetAll,taskBoundariesMatrix[1,taskId],taskBoundariesMatrix[2,taskId],
                                                              taskBoundariesMatrix[3,taskId],taskBoundariesMatrix[4,taskId]))
    }
    
    if (savedQueries[queryId] >= taskBoundariesMatrix[1,taskId] || (!is.infinite(closestResetAll) && savedQueries[queryId] > closestResetAll)) { #earlier than start
      if (savedQueries[queryId] <= taskBoundariesMatrix[2,taskId]) 
      {
        resultQueries = cbind(resultQueries, c(savedQueries[queryId], taskBoundariesMatrix[3,taskId]))
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
  taskBoundariesMatrix = cbind(taskBoundariesMatrix, c(jsonData[["startTimeStamp"]], jsonData[["endTimeStamp"]], jsonData[["name"]], jsonData[["_id"]]))
}

taskBoundariesMatrix = taskBoundariesMatrix[,order(taskBoundariesMatrix[1,])]
taskBoundaries = as.vector(taskBoundariesMatrix)

for (member in c(0,1)) {
  
  #actions
  actions = c()
  allActions = c()
  for (line in actionLines)
  {
    actionData = fromJSON(line)
    if (is.null(actionData[["teamId"]]) || actionData[["teamId"]] != 4 || actionData[["memberId"]] != member || length(actionData[["events"]]) <= 0) {
      next
    }
      
    allActions = cbind(allActions, sapply(actionData[["events"]], 
                                          function(event) { c(ifelse(is.null(event[["timestamp"]]), NA, event[["timestamp"]]),
                                                              ifelse(is.null(event[["type"]]), NA, event[["type"]]),
                                                              ifelse(is.null(event[["category"]]), NA, event[["category"]]),
                                                              ifelse(is.null(event[["value"]]), NA, event[["value"]])) }))
    if (is.element(actionData[["taskId"]], validTasks)) {
      actions = cbind(actions, sapply(actionData[["events"]], 
                                      function(event) { c(event[["timestamp"]],event[["type"]],event[["category"]],
                                      ifelse(is.null(event[["value"]]), "NA", event[["value"]])) }))
    }
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
    
    submissions = cbind(submissions, c(subData[["timestamp"]], ifelse(is.null(subData[["correct"]]), 0, subData[["correct"]]),
                                       subData[["taskId"]]))
  }
  
  #saved tasks
  baseSavedQPath = "..\\..\\..\\Logs\\2019-01-09_VBS2019\\"
  allSavedQueries = as.numeric(sub(".json$", "", list.files(paste0(baseSavedQPath, ifelse(member == 0,"SIRIUS-PC","PREMEK-NTB"), "\\QueriesLog"))))
  
  newQueriesAndActions = filterQueries(taskBoundariesMatrix, actions, allSavedQueries)
  filteredSavedQueries = newQueriesAndActions[[1]]
  actions = newQueriesAndActions[[2]]
  filteredTaskBoundaries = newQueriesAndActions[[3]]
  
  write.table(filteredTaskBoundaries, paste0("TaskBoundaries_member_",member,".csv"), row.names = FALSE, sep = ";")
  write.table(t(submissions), paste0("Submissions_member_",member,".csv"), col.names = FALSE, row.names = FALSE, sep = ";")
  write.table(t(actions[,actions[3,] == "Browsing"]), paste0("Actions_member_",member,".csv"), col.names = FALSE, row.names = FALSE, sep = ";")
  #task order
  write.table(sapply(validTasks, function(taskId) {filteredTaskBoundaries[filteredTaskBoundaries[,"TaskId"]==taskId,][[4]]}),
              paste0("Tasks_order_member_",member,".csv"), col.names = TRUE, row.names = FALSE, sep = ";")
  
  queryResultsFile = file(paste0("ResultRankings_",ifelse(member == 0,"SIRIUS-PC","PREMEK-NTB"),".txt"),open="r")
  queryResults = unname(sapply(readLines(queryResultsFile),
                        function(line) {
                          queryResJson = fromJSON(line)
                          return (t(c(queryResJson[["queryTimestamp"]], queryResJson[["topVideoPosition"]],queryResJson[["topShotPosition"]])))
                        }))
  close(queryResultsFile)
  
  queryResultsBeforeFilterFile = file(paste0("ResultRankings_",ifelse(member == 0,"SIRIUS-PC","PREMEK-NTB"),"_BeforeCountFilter.txt"),open="r")
  queryResultsBF = unname(sapply(readLines(queryResultsBeforeFilterFile),
                               function(line) {
                                 queryResJson = fromJSON(line)
                                 return (t(c(queryResJson[["queryTimestamp"]], queryResJson[["topVideoPosition"]],queryResJson[["topShotPosition"]])))
                               }))
  close(queryResultsBeforeFilterFile)
  
  #transform queries
  transformedQueries = c()
  firstKwValue =""; secondKwValue = ""
  prevFirstKwCount = 0; prevSecondKwCount = 0; prevMainModel = 0
  allSavedQueries = allSavedQueries[order(allSavedQueries)]
  for (queryTS in allSavedQueries) {
    
    queryFile = file(paste0(baseSavedQPath,ifelse(member == 0,"SIRIUS-PC","PREMEK-NTB"),"\\QueriesLog\\",queryTS,".json"),open="r")
    queryData = fromJSON(file = queryFile)
    close(queryFile)
    
    mainModel = queryData[["primarytemporalquery"]]
    firstIsMain = mainModel == 0
    first = ifelse(firstIsMain, "formerquery", "latterquery")
    second = ifelse(!firstIsMain, "formerquery", "latterquery")
    similarity = queryData[["bitemporalsimilarityquery"]]
    
    firstKwCount = length(similarity[["keywordquery"]][[first]][["synsetgroups"]])
    secondKwCount = length(similarity[["keywordquery"]][[second]][["synsetgroups"]])
    
    #model order just switched
    if (mainModel != prevMainModel) {
      temp = firstKwValue
      firstKwValue = secondKwValue
      secondKwValue = temp
    } else {
      #Keywords matching logic
      potentialKw = allActions[,allActions[1,] == queryTS & allActions[2,] == "Concept" & allActions[3,] == "Text"]
      
      if (prevFirstKwCount != firstKwCount) {
        firstKwValue = ifelse(length(potentialKw) > 0, potentialKw[[4]], ifelse(firstKwCount <=0, "", firstKwValue))
      } 
      
      if (prevSecondKwCount != secondKwCount) {
        secondKwValue = ifelse(length(potentialKw) > 0, potentialKw[[4]], ifelse(secondKwCount <=0, "", secondKwValue))
      }
    }
    prevFirstKwCount = firstKwCount
    prevSecondKwCount = secondKwCount
    prevMainModel = mainModel
    
    if (!is.element(queryTS, filteredSavedQueries[1,])) {
      next
    }
    
    taskName = filteredSavedQueries[2,filteredSavedQueries[1,] == queryTS]
    taskId = filteredTaskBoundaries[filteredTaskBoundaries[,4]==taskName,][[5]]
    firstSucSub = min(submissions[,submissions[3,]==taskId & submissions[2,]=="TRUE"][1])
    transformedQuery = list(
      "TimeStamp" = queryTS,
      "TaskName" = taskName,
      "TopVideoPosition" = queryResults[,queryResults[1,] == queryTS][2],
      "TopShotPosition" = queryResults[,queryResults[1,] == queryTS][3],
      "TopVideoPositionBeforeFilter" = queryResultsBF[,queryResultsBF[1,] == queryTS][2],
      "TopShotPositionBeforeFilter" = queryResultsBF[,queryResultsBF[1,] == queryTS][3],
      "KW_1_Count" = firstKwCount,
      "KW_1_Words" = ifelse(firstKwValue != "NA" && !is.na(firstKwValue), firstKwValue, ""),
      "KW_1_IDS" = paste(sapply(similarity[["keywordquery"]][[first]][["synsetgroups"]], 
                          function(g) { paste(sapply(g[["synsets"]], function(s) { s[["synsetid"]]}),collapse ="+")}),collapse ="|"),
      "KW_2_Count" = secondKwCount,
      "KW_2_Words" = ifelse(secondKwValue != "NA" && !is.na(secondKwValue), secondKwValue, ""),
      "KW_2_IDS" = paste(sapply(similarity[["keywordquery"]][[second]][["synsetgroups"]], 
                                function(g) { paste(sapply(g[["synsets"]], function(s) { s[["synsetid"]]}),collapse ="+")}),collapse ="|"),
      "Color_1" = length(similarity[["colorsketchquery"]][[first]][["colorsketchellipses"]]),
      "Color_2" = length(similarity[["colorsketchquery"]][[second]][["colorsketchellipses"]]),
      "Face_1" = length(similarity[["facesketchquery"]][[first]][["colorsketchellipses"]]),
      "Face_2" = length(similarity[["facesketchquery"]][[second]][["colorsketchellipses"]]),
      "Text_1" = length(similarity[["textsketchquery"]][[first]][["colorsketchellipses"]]),
      "Text_2" = length(similarity[["textsketchquery"]][[second]][["colorsketchellipses"]]),
      "Semantic_1" = ifelse(length(similarity[["semanticexamplequery"]][[first]][["positiveexampleids"]]) > 0, "on",
                            ifelse(length(similarity[["semanticexamplequery"]][[first]][["externalimages"]]) > 0, "external", "off")),
      "Semantic_2" = ifelse(length(similarity[["semanticexamplequery"]][[second]][["positiveexampleids"]]) > 0, "on", 
                            ifelse(length(similarity[["semanticexamplequery"]][[second]][["externalimages"]]) > 0, "external", "off")),
      "SortedBy" = switch(as.numeric(queryData[[ifelse(queryData[["primarytemporalquery"]] == 0, "formerfusionquery", "latterfusionquery")]][["sortingsimilaritymodel"]]) + 1,
                          "Keyword", "ColorSketch", "Face", "Text", "Semantic", "None"),
      "TaskStart" = filteredTaskBoundaries[filteredTaskBoundaries[,4]==taskName,][[2]],
      "TaskEnd" = filteredTaskBoundaries[filteredTaskBoundaries[,4]==taskName,][[3]],
      "FirstSuccessfulSubmission" = firstSucSub,
      "IsAfterSuccessfulSubmission" = queryTS > firstSucSub
    )
    
    transformedQueries = rbind(transformedQueries, transformedQuery)
  }
  write.table(transformedQueries, paste0("Queries_member_",member,".csv"), row.names = FALSE, sep = ";")
}





