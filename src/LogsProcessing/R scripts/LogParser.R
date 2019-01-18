#install.packages("rjson")


init.graph = function (file.name, width, height, pointsize) {
  dpi = 72
  pdf(file.name, width = width / dpi, height = height / dpi, pointsize = pointsize)
  par(mar=c(2.9,6,2,2), mgp=c(3.9,0.5,0))
}

graph = function (file.name, taskBoundaries, xlab, ylab, taskColors, func.before =function() {}, ylim=NULL, xaxt="n", yaxt="n",
                  func.after=function() {}, xlim=NULL, log="", width=1000, height=200, pointsize = 13.5, type="o", lty=1, lineWidth = 4) {
  
  init.graph(file.name, width, height, pointsize)
  
  tryCatch({
    
    #bit of a hack to force first line to be on top of axis lines
    plot(c(), c(), type="n", ylim=ylim, xlim=xlim, xlab=xlab, ylab=ylab, xaxt=xaxt, yaxt=yaxt, tck=1, fg="grey", log=log)
    func.before()
    
    abline(v=taskBoundaries, col=taskColors)
    
    func.after()
    
  }, finally = {
    dev.off()
  }, error = function(e) {
    print(e)
  })
}


filterQueries = function(taskBoundariesMatrix, actions, savedQueries) {
  
  resultQueries = c()
  taskId = 1
  closestResetAll = suppressWarnings(max(as.numeric(actions[,actions[2,] == "ResetAll" && actions[1,] < taskBoundariesMatrix[1,taskId]][1,])))
  for (queryId in 1:length(savedQueries))
  {
    if (savedQueries[queryId] > taskBoundariesMatrix[2,taskId] && taskId < length(taskBoundariesMatrix[1,])) { #after end
      taskId = taskId + 1
      closestResetAll = suppressWarnings(max(as.numeric(actions[,actions[2,] == "ResetAll" && actions[1,] < taskBoundariesMatrix[1,taskId]][1,])))
    }
    
    if (savedQueries[queryId] >= taskBoundariesMatrix[1,taskId] || (!is.infinite(closestResetAll) && savedQueries[queryId] > closestResetAll)) { #earlier than start
      if (savedQueries[queryId] <= taskBoundariesMatrix[2,taskId]) 
      {
        resultQueries = append(resultQueries, savedQueries[queryId])  
      }
    } 
  }
  
  return(resultQueries)
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

origTasks = validTasks
for (day in c(1,2)) {

  validTasks = origTasks[c((13*(day-1)+1):(13*day))]
 
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
  
  taskColors = rep(c("red", "blue", "green", "orange", "gray", "yellow"),each=2)
  graph(paste0("graph_day",day,".pdf"), taskBoundaries, "Time", "Actions", 
      taskColors,
      xlim = c(min(taskBoundaries) - 10, max(taskBoundaries) + 10), ylim=c(0.7,6.3),
      func.after = function() {
        
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
            
            actions = cbind(actions, sapply(actionData[["events"]], function(event) { c(event[["timestamp"]], event[["type"]], event[["category"]]) }))
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
          savedQueries = filterQueries(taskBoundariesMatrix, actions, savedQueries)
          
          cex=0.7
          points(actions[1,], rep(3*member+1, length(actions[1,])), lty=1, pch=20, cex=cex)
          subColors = sapply(submissions[2,], function (success) { ifelse(success == 1, "red", "black")})
          points(submissions[1,], rep(3*member+2, length(submissions[1,])), lty=1, pch=20, col=subColors, cex=cex)
          points(savedQueries, rep(3*member+3, length(savedQueries)), lty=1, pch=20, cex=cex)
        }
        #axis(1, at=taskBoundaries, labels=rep(1:(length(taskBoundaries)/2), each=2), col = "grey", tck=0, cex.axis=0.5)
        axis(2, at=c(1:6), labels=paste0(c("All\nMember ", "Submissions\nMember ", "Saved queries\nMember "),c(rep(0,3),rep(1,3))), col = "grey", tck=0, cex.axis=0.5, las=1)
      }
  )   
}




